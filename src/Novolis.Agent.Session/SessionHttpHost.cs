using System.Net;
using System.Text;
using System.Text.Json;

namespace Novolis.Agent.Session;

/// <summary>
/// Super-slim localhost HTTP surface for session.* — no Kestrel.
/// REST JSON + SSE events so agents can talk to one EXE (Avalonia or headless).
/// </summary>
public sealed class SessionHttpHost : IAsyncDisposable, ISessionTransport
{
    private readonly IGameSession _session;
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;
    private readonly object _sseGate = new();
    private readonly List<StreamWriter> _sseClients = new();
    private readonly Action<SessionDecisionEventDto> _onDecision;
    private readonly Action<SessionChangedEventDto> _onChanged;
    private readonly Action<SessionActionResultEventDto> _onActionResult;

    private SessionHttpHost(IGameSession session, string prefix)
    {
        _session = session;
        BaseUrl = prefix.TrimEnd('/');
        var listenPrefix = prefix.EndsWith('/') ? prefix : prefix + "/";
        _listener = new HttpListener();
        _listener.Prefixes.Add(listenPrefix);
        _onDecision = e => BroadcastSse(SessionMethodNames.Decision, e);
        _onChanged = e => BroadcastSse(SessionMethodNames.Changed, e);
        _onActionResult = e => BroadcastSse(SessionMethodNames.ActionResult, e);
        _session.Decision += _onDecision;
        _session.Changed += _onChanged;
        _session.ActionResult += _onActionResult;
        _listener.Start();
        _loop = Task.Run(() => ListenAsync(_cts.Token));
        try
        {
            File.WriteAllText(
                SessionEndpoints.HttpMarkerPath,
                $"{Environment.ProcessId}\n{BaseUrl}\n");
        }
        catch
        {
            // ignore marker failures
        }
    }

    public string Kind => "http";

    public string BaseUrl { get; }

    public static SessionHttpHost Attach(IGameSession session, int? port = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        var p = port ?? SessionEndpoints.ResolveHttpPort();
        return new SessionHttpHost(session, $"http://127.0.0.1:{p}/");
    }

    public static SessionHttpHost? TryAttachFromEnvironment(IGameSession session)
    {
        if (!SessionEndpoints.IsHttpEnabledByEnvironment())
            return null;
        return Attach(session);
    }

    public ValueTask StartAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

    public async ValueTask StopAsync(CancellationToken cancellationToken = default) =>
        await DisposeAsync().ConfigureAwait(false);

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                _ = Task.Run(() => HandleAsync(context, cancellationToken), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // shutting down
        }
        catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
        {
            // shutting down
        }
        catch (ObjectDisposedException)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            try
            {
                await File.WriteAllTextAsync(
                    SessionEndpoints.HttpMarkerPath + ".error",
                    ex.ToString(),
                    CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }
        }
    }

    private async Task HandleAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        try
        {
            var req = context.Request;
            var path = (req.Url?.AbsolutePath ?? "/").TrimEnd('/');
            if (path.Length == 0) path = "/";

            if (req.HttpMethod == "OPTIONS")
            {
                WriteCors(context.Response);
                context.Response.StatusCode = 204;
                context.Response.Close();
                return;
            }

            if (path is "/health" or "/session/health")
            {
                await WriteJsonAsync(context.Response, 200, new { ok = true, transport = Kind, baseUrl = BaseUrl }, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            if (path == "/session/events" && req.HttpMethod == "GET")
            {
                await HandleSseAsync(context, cancellationToken).ConfigureAwait(false);
                return;
            }

            object result;
            if (path == "/session/hello" && req.HttpMethod == "GET")
                result = _session.Hello();
            else if (path == "/session/snapshot" && req.HttpMethod == "GET")
                result = _session.Snapshot();
            else if (path == "/session/actions" && req.HttpMethod == "GET")
                result = _session.Actions();
            else if (path == "/session/continue" && req.HttpMethod == "POST")
                result = _session.Continue();
            else if (path == "/session/subscribe" && req.HttpMethod == "POST")
            {
                _session.Subscribe();
                result = new SessionSubscribeResponseDto { Ok = true };
            }
            else if (path == "/session/command" && req.HttpMethod == "POST")
            {
                using var doc = await ReadJsonAsync(req, cancellationToken).ConfigureAwait(false);
                result = _session.Execute(SessionJsonDispatcher.ParseCommand(doc.RootElement));
            }
            else if (path == "/session/rpc" && req.HttpMethod == "POST")
            {
                using var doc = await ReadJsonAsync(req, cancellationToken).ConfigureAwait(false);
                var method = doc.RootElement.TryGetProperty("method", out var m) ? m.GetString() : null;
                result = SessionJsonDispatcher.Dispatch(_session, method, doc.RootElement);
            }
            else
            {
                await WriteJsonAsync(context.Response, 404, new { ok = false, error = $"not found {path}" }, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            await WriteJsonAsync(context.Response, 200, new { ok = true, result }, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            try
            {
                await WriteJsonAsync(context.Response, 500, new { ok = false, error = ex.Message }, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                // connection dead
            }
        }
    }

    private async Task HandleSseAsync(HttpListenerContext context, CancellationToken cancellationToken)
    {
        _session.Subscribe();
        var response = context.Response;
        WriteCors(response);
        response.StatusCode = 200;
        response.ContentType = "text/event-stream";
        response.Headers["Cache-Control"] = "no-cache";
        response.Headers["Connection"] = "keep-alive";
        response.SendChunked = true;

        var writer = new StreamWriter(response.OutputStream, new UTF8Encoding(false)) { AutoFlush = true };
        lock (_sseGate)
        {
            _sseClients.Add(writer);
        }

        try
        {
            await writer.WriteAsync(": connected\n\n").ConfigureAwait(false);
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(15000, cancellationToken).ConfigureAwait(false);
                await writer.WriteAsync(": ping\n\n").ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
        catch
        {
            // client gone
        }
        finally
        {
            lock (_sseGate)
            {
                _sseClients.Remove(writer);
            }

            try { writer.Dispose(); } catch { /* ignore */ }
            try { response.Close(); } catch { /* ignore */ }
        }
    }

    private void BroadcastSse(string eventName, object payload)
    {
        string data;
        try
        {
            data = JsonSerializer.Serialize(payload, SessionJsonDispatcher.JsonOptions);
        }
        catch
        {
            return;
        }

        var frame = $"event: {eventName}\ndata: {data}\n\n";
        List<StreamWriter> clients;
        lock (_sseGate)
        {
            clients = _sseClients.ToList();
        }

        foreach (var client in clients)
        {
            try
            {
                client.Write(frame);
            }
            catch
            {
                lock (_sseGate)
                {
                    _sseClients.Remove(client);
                }

                try { client.Dispose(); } catch { /* ignore */ }
            }
        }
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpListenerRequest request, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(text))
            return JsonDocument.Parse("{}");
        return JsonDocument.Parse(text);
    }

    private static async Task WriteJsonAsync(
        HttpListenerResponse response,
        int status,
        object payload,
        CancellationToken cancellationToken)
    {
        WriteCors(response);
        response.StatusCode = status;
        response.ContentType = "application/json; charset=utf-8";
        var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, SessionJsonDispatcher.JsonOptions);
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        response.Close();
    }

    private static void WriteCors(HttpListenerResponse response)
    {
        response.Headers["Access-Control-Allow-Origin"] = "*";
        response.Headers["Access-Control-Allow-Methods"] = "GET,POST,OPTIONS";
        response.Headers["Access-Control-Allow-Headers"] = "content-type";
    }

    public async ValueTask DisposeAsync()
    {
        _session.Decision -= _onDecision;
        _session.Changed -= _onChanged;
        _session.ActionResult -= _onActionResult;
        await _cts.CancelAsync().ConfigureAwait(false);
        try { _listener.Stop(); } catch { /* ignore */ }
        try { _listener.Close(); } catch { /* ignore */ }
        lock (_sseGate)
        {
            foreach (var c in _sseClients)
            {
                try { c.Dispose(); } catch { /* ignore */ }
            }

            _sseClients.Clear();
        }

        try { await _loop.ConfigureAwait(false); } catch { /* ignore */ }
        _cts.Dispose();
        try { File.Delete(SessionEndpoints.HttpMarkerPath); } catch { /* ignore */ }
    }
}
