using System.Text.Json;

namespace Novolis.Agent.Session;

/// <summary>
/// Stdio JSONL host: one JSON object per line with method + args.
/// Headless agents share the same session protocol as LocalIpc / HTTP / TCP.
/// </summary>
public sealed class SessionStdioHost : ISessionTransport
{
    private readonly IGameSession _session;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public SessionStdioHost(IGameSession session, TextReader? input = null, TextWriter? output = null)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _input = input ?? Console.In;
        _output = output ?? Console.Out;
    }

    public string Kind => "stdio-jsonl";

    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
        return ValueTask.CompletedTask;
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        if (_cts is null) return;
        await _cts.CancelAsync().ConfigureAwait(false);
        if (_loop is not null)
        {
            try { await _loop.ConfigureAwait(false); }
            catch (OperationCanceledException) { }
        }
    }

    public async Task RunUntilEofAsync(CancellationToken cancellationToken = default)
    {
        await RunAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await _input.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null) break;
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                var method = root.TryGetProperty("method", out var m) ? m.GetString() : null;
                var id = root.TryGetProperty("id", out var idEl) ? idEl.GetInt64() : 0L;
                var result = SessionJsonDispatcher.Dispatch(_session, method, root);
                await WriteAsync(new { id, ok = true, result }, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await WriteAsync(new { ok = false, error = ex.Message }, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task WriteAsync(object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, SessionJsonDispatcher.JsonOptions);
        await _output.WriteLineAsync(json.AsMemory(), cancellationToken).ConfigureAwait(false);
        await _output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
}
