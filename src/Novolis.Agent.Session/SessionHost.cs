using Novolis.Transports.LocalIpc;

namespace Novolis.Agent.Session;

/// <summary>Hosts session.* request/response and fans out events to subscribed connections.</summary>
public sealed class SessionHost : IAsyncDisposable, ISessionTransport
{
    private readonly IGameSession _session;
    private readonly LocalIpcEndpoint _endpoint;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _listenTask;
    private readonly object _subscribersGate = new();
    private readonly HashSet<ILocalIpcConnection> _subscribers = new();
    private long _eventSequence;
    private ILocalIpcListener? _listener;

    private SessionHost(IGameSession session, LocalIpcEndpoint endpoint)
    {
        _session = session;
        _endpoint = endpoint;
        _session.Decision += OnDecision;
        _session.Changed += OnChanged;
        _session.ActionResult += OnActionResult;
        _listenTask = Task.Run(() => ListenAsync(_cts.Token));
    }

    public string Kind => "local-ipc";

    public static SessionHost Attach(IGameSession session, string? endpointAddress = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        var endpoint = SessionEndpoints.CreateDefault(endpointAddress);
        return new SessionHost(session, endpoint);
    }

    public static SessionHost? TryAttachFromEnvironment(IGameSession session, string? preferredAddress = null)
    {
        if (!SessionEndpoints.IsEnabledByEnvironment())
            return null;
        return Attach(session, preferredAddress);
    }

    public ValueTask StartAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

    public async ValueTask StopAsync(CancellationToken cancellationToken = default) =>
        await DisposeAsync().ConfigureAwait(false);

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        try
        {
            await File.WriteAllTextAsync(
                    SessionEndpoints.HostMarkerPath,
                    $"{Environment.ProcessId}\n{_endpoint.Kind}\n{_endpoint.Address}\n",
                    cancellationToken)
                .ConfigureAwait(false);

            _listener = LocalIpcTransport.CreateListener(_endpoint);
            while (!cancellationToken.IsCancellationRequested)
            {
                var connection = await _listener.AcceptAsync(cancellationToken).ConfigureAwait(false);
                _ = Task.Run(() => HandleConnectionAsync(connection, cancellationToken), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            try
            {
                await File.WriteAllTextAsync(
                    SessionEndpoints.HostMarkerPath + ".error",
                    ex.ToString(),
                    CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }
        }
    }

    private async Task HandleConnectionAsync(ILocalIpcConnection connection, CancellationToken cancellationToken)
    {
        await using (connection)
        {
            try
            {
                await foreach (var frame in connection.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (frame.Kind != SessionRpcMessageKinds.Request)
                        continue;

                    try
                    {
                        await DispatchAsync(connection, frame, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        await ReplyFaultAsync(connection, frame, ex.Message, cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // connection closed
            }
            finally
            {
                lock (_subscribersGate)
                {
                    _subscribers.Remove(connection);
                }
            }
        }
    }

    private async Task DispatchAsync(
        ILocalIpcConnection connection,
        LocalIpcFrame frame,
        CancellationToken cancellationToken)
    {
        switch (frame.Name)
        {
            case SessionMethodNames.Hello:
            {
                var req = SessionProtocolCodec.Deserialize<SessionHelloRequestDto>(frame.Payload);
                var res = _session.Hello();
                res.Sequence = req.Sequence;
                await ReplyAsync(connection, frame, res, cancellationToken).ConfigureAwait(false);
                break;
            }
            case SessionMethodNames.Snapshot:
            {
                var req = SessionProtocolCodec.Deserialize<SessionSnapshotRequestDto>(frame.Payload);
                var res = _session.Snapshot();
                res.Sequence = req.Sequence;
                await ReplyAsync(connection, frame, res, cancellationToken).ConfigureAwait(false);
                break;
            }
            case SessionMethodNames.Actions:
            {
                var req = SessionProtocolCodec.Deserialize<SessionActionsRequestDto>(frame.Payload);
                var res = _session.Actions();
                res.Sequence = req.Sequence;
                await ReplyAsync(connection, frame, res, cancellationToken).ConfigureAwait(false);
                break;
            }
            case SessionMethodNames.Command:
            {
                var req = SessionProtocolCodec.Deserialize<SessionCommandRequestDto>(frame.Payload);
                var res = _session.Execute(req.Command);
                res.Sequence = req.Sequence;
                await ReplyAsync(connection, frame, res, cancellationToken).ConfigureAwait(false);
                break;
            }
            case SessionMethodNames.Continue:
            {
                var req = SessionProtocolCodec.Deserialize<SessionContinueRequestDto>(frame.Payload);
                var res = _session.Continue();
                res.Sequence = req.Sequence;
                await ReplyAsync(connection, frame, res, cancellationToken).ConfigureAwait(false);
                break;
            }
            case SessionMethodNames.Subscribe:
            {
                var req = SessionProtocolCodec.Deserialize<SessionSubscribeRequestDto>(frame.Payload);
                _session.Subscribe();
                lock (_subscribersGate)
                {
                    _subscribers.Add(connection);
                }

                await ReplyAsync(
                        connection,
                        frame,
                        new SessionSubscribeResponseDto { Sequence = req.Sequence, Ok = true },
                        cancellationToken)
                    .ConfigureAwait(false);
                break;
            }
            default:
                await ReplyFaultAsync(connection, frame, $"Unknown method {frame.Name}", cancellationToken)
                    .ConfigureAwait(false);
                break;
        }
    }

    private static ValueTask ReplyAsync<T>(
        ILocalIpcConnection connection,
        LocalIpcFrame request,
        T payload,
        CancellationToken cancellationToken) =>
        connection.SendMessageAsync(request.Sequence, SessionRpcMessageKinds.Response, request.Name, payload, cancellationToken);

    private static ValueTask ReplyFaultAsync(
        ILocalIpcConnection connection,
        LocalIpcFrame request,
        string message,
        CancellationToken cancellationToken) =>
        connection.SendMessageAsync(
            request.Sequence,
            SessionRpcMessageKinds.Fault,
            request.Name,
            new SessionFaultDto { Sequence = request.Sequence, Message = message },
            cancellationToken);

    private void OnDecision(SessionDecisionEventDto evt)
    {
        evt.Sequence = NextEventSequence();
        Broadcast(SessionMethodNames.Decision, evt);
    }

    private void OnChanged(SessionChangedEventDto evt)
    {
        evt.Sequence = NextEventSequence();
        Broadcast(SessionMethodNames.Changed, evt);
    }

    private void OnActionResult(SessionActionResultEventDto evt)
    {
        evt.Sequence = NextEventSequence();
        Broadcast(SessionMethodNames.ActionResult, evt);
    }

    private long NextEventSequence() => Interlocked.Increment(ref _eventSequence);

    private void Broadcast<T>(string name, T payload)
    {
        byte[] bytes;
        try
        {
            bytes = SessionProtocolCodec.Serialize(payload);
        }
        catch
        {
            return;
        }

        List<ILocalIpcConnection> targets;
        lock (_subscribersGate)
        {
            targets = _subscribers.ToList();
        }

        foreach (var connection in targets)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await connection.SendAsync(
                            new LocalIpcFrame(0, SessionRpcMessageKinds.Event, name, bytes),
                            CancellationToken.None)
                        .ConfigureAwait(false);
                }
                catch
                {
                    lock (_subscribersGate)
                    {
                        _subscribers.Remove(connection);
                    }
                }
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        _session.Decision -= OnDecision;
        _session.Changed -= OnChanged;
        _session.ActionResult -= OnActionResult;
        await _cts.CancelAsync().ConfigureAwait(false);
        if (_listener is not null)
            await _listener.DisposeAsync().ConfigureAwait(false);
        try
        {
            await _listenTask.ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }

        _cts.Dispose();
    }
}
