using Novolis.Transports.LocalIpc;

namespace Novolis.Agent.Session;

/// <summary>Client for session.* over LocalIpc frames.</summary>
public sealed class SessionClient : IAsyncDisposable
{
    private readonly ILocalIpcConnection _connection;
    private long _sequence;
    private readonly object _gate = new();
    private readonly Dictionary<long, TaskCompletionSource<LocalIpcFrame>> _pending = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _readLoop;

    public event Action<string, byte[]>? EventReceived;

    private SessionClient(ILocalIpcConnection connection)
    {
        _connection = connection;
        _readLoop = Task.Run(ReadLoopAsync);
    }

    public static async Task<SessionClient> ConnectAsync(
        LocalIpcEndpoint endpoint,
        CancellationToken cancellationToken = default)
    {
        var client = LocalIpcTransport.CreateClient();
        var connection = await client.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
        return new SessionClient(connection);
    }

    public async Task<SessionHelloResponseDto> HelloAsync(CancellationToken cancellationToken = default)
    {
        var seq = NextSequence();
        var request = new SessionHelloRequestDto { Sequence = seq };
        var frame = await RequestAsync(seq, SessionMethodNames.Hello, request, cancellationToken)
            .ConfigureAwait(false);
        return SessionProtocolCodec.Deserialize<SessionHelloResponseDto>(frame.Payload);
    }

    public async Task<SessionSnapshotDto> SnapshotAsync(CancellationToken cancellationToken = default)
    {
        var seq = NextSequence();
        var request = new SessionSnapshotRequestDto { Sequence = seq };
        var frame = await RequestAsync(seq, SessionMethodNames.Snapshot, request, cancellationToken)
            .ConfigureAwait(false);
        return SessionProtocolCodec.Deserialize<SessionSnapshotDto>(frame.Payload);
    }

    public async Task<SessionActionsResponseDto> ActionsAsync(CancellationToken cancellationToken = default)
    {
        var seq = NextSequence();
        var request = new SessionActionsRequestDto { Sequence = seq };
        var frame = await RequestAsync(seq, SessionMethodNames.Actions, request, cancellationToken)
            .ConfigureAwait(false);
        return SessionProtocolCodec.Deserialize<SessionActionsResponseDto>(frame.Payload);
    }

    public async Task<SessionCommandResultDto> CommandAsync(
        SessionCommandDto command,
        CancellationToken cancellationToken = default)
    {
        var seq = NextSequence();
        var request = new SessionCommandRequestDto { Sequence = seq, Command = command };
        var frame = await RequestAsync(seq, SessionMethodNames.Command, request, cancellationToken)
            .ConfigureAwait(false);
        return SessionProtocolCodec.Deserialize<SessionCommandResultDto>(frame.Payload);
    }

    public async Task<SessionCommandResultDto> ContinueAsync(CancellationToken cancellationToken = default)
    {
        var seq = NextSequence();
        var request = new SessionContinueRequestDto { Sequence = seq };
        var frame = await RequestAsync(seq, SessionMethodNames.Continue, request, cancellationToken)
            .ConfigureAwait(false);
        return SessionProtocolCodec.Deserialize<SessionCommandResultDto>(frame.Payload);
    }

    public async Task<SessionSubscribeResponseDto> SubscribeAsync(CancellationToken cancellationToken = default)
    {
        var seq = NextSequence();
        var request = new SessionSubscribeRequestDto { Sequence = seq };
        var frame = await RequestAsync(seq, SessionMethodNames.Subscribe, request, cancellationToken)
            .ConfigureAwait(false);
        return SessionProtocolCodec.Deserialize<SessionSubscribeResponseDto>(frame.Payload);
    }

    private long NextSequence()
    {
        lock (_gate)
        {
            return ++_sequence;
        }
    }

    private async Task<LocalIpcFrame> RequestAsync<T>(
        long sequence,
        string method,
        T payload,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<LocalIpcFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_gate)
        {
            _pending[sequence] = tcs;
        }

        await using var reg = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        await _connection.SendMessageAsync(sequence, SessionRpcMessageKinds.Request, method, payload, cancellationToken)
            .ConfigureAwait(false);
        return await tcs.Task.ConfigureAwait(false);
    }

    private async Task ReadLoopAsync()
    {
        try
        {
            await foreach (var frame in _connection.ReadAllAsync(_cts.Token).ConfigureAwait(false))
            {
                if (frame.Kind is SessionRpcMessageKinds.Response or SessionRpcMessageKinds.Fault)
                {
                    TaskCompletionSource<LocalIpcFrame>? tcs;
                    lock (_gate)
                    {
                        _pending.Remove(frame.Sequence, out tcs);
                    }

                    if (frame.Kind == SessionRpcMessageKinds.Fault)
                    {
                        var fault = SessionProtocolCodec.Deserialize<SessionFaultDto>(frame.Payload);
                        tcs?.TrySetException(new InvalidOperationException(fault.Message));
                    }
                    else
                    {
                        tcs?.TrySetResult(frame);
                    }
                }
                else if (frame.Kind == SessionRpcMessageKinds.Event)
                {
                    EventReceived?.Invoke(frame.Name, frame.Payload);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            lock (_gate)
            {
                foreach (var tcs in _pending.Values)
                {
                    tcs.TrySetException(ex);
                }

                _pending.Clear();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync().ConfigureAwait(false);
        try
        {
            await _readLoop.ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }

        await _connection.DisposeAsync().ConfigureAwait(false);
        _cts.Dispose();
    }
}
