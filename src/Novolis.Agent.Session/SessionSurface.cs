namespace Novolis.Agent.Session;

/// <summary>
/// One-shot attach for all session transports so Avalonia and headless share the same surface.
/// LocalIpc (MessagePack) + HTTP REST/SSE (default) + optional TCP JSONL.
/// </summary>
public sealed class SessionSurface : IAsyncDisposable
{
    private readonly List<IAsyncDisposable> _hosts = new();

    private SessionSurface()
    {
    }

    public SessionHost? LocalIpc { get; private set; }

    public SessionHttpHost? Http { get; private set; }

    public SessionTcpJsonlHost? Tcp { get; private set; }

    public string? HttpBaseUrl => Http?.BaseUrl;

    public int? TcpPort => Tcp?.Port;

    /// <summary>
    /// Attach transports from environment. Returns null when nothing is enabled.
    /// Prefer calling this once from captain CLI and Avalonia after creating <see cref="IGameSession"/>.
    /// </summary>
    public static SessionSurface? TryAttachFromEnvironment(
        IGameSession session,
        string? preferredPipeName = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        var surface = new SessionSurface();
        var any = false;

        if (SessionEndpoints.IsEnabledByEnvironment())
        {
            var ipc = SessionHost.Attach(session, preferredPipeName ?? SessionEndpoints.DefaultPipeName);
            surface.LocalIpc = ipc;
            surface._hosts.Add(ipc);
            any = true;
        }

        // HTTP is the wide agent surface — on whenever session agent is on, unless explicitly disabled.
        if (SessionEndpoints.IsHttpEnabledByEnvironment())
        {
            var http = SessionHttpHost.Attach(session);
            surface.Http = http;
            surface._hosts.Add(http);
            any = true;
        }

        if (SessionEndpoints.IsTcpEnabledByEnvironment())
        {
            var tcp = SessionTcpJsonlHost.Attach(session);
            surface.Tcp = tcp;
            surface._hosts.Add(tcp);
            any = true;
        }

        return any ? surface : null;
    }

    /// <summary>
    /// Attach all supported transports unconditionally (no env gating).
    /// Used so the same EXE can be controlled mid-game from Avalonia or headless/agents.
    /// </summary>
    public static SessionSurface? AttachAll(
        IGameSession session,
        string? preferredPipeName = null,
        int? httpPort = null,
        int? tcpPort = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        var surface = new SessionSurface();
        var any = false;

        try
        {
            var ipc = SessionHost.Attach(session, preferredPipeName ?? SessionEndpoints.DefaultPipeName);
            surface.LocalIpc = ipc;
            surface._hosts.Add(ipc);
            any = true;
        }
        catch
        {
            // ignore per-transport bind failures
        }

        try
        {
            var http = SessionHttpHost.Attach(session, httpPort);
            surface.Http = http;
            surface._hosts.Add(http);
            any = true;
        }
        catch
        {
            // ignore per-transport bind failures
        }

        try
        {
            var tcp = SessionTcpJsonlHost.Attach(session, tcpPort);
            surface.Tcp = tcp;
            surface._hosts.Add(tcp);
            any = true;
        }
        catch
        {
            // ignore per-transport bind failures
        }

        return any ? surface : null;
    }

    public async ValueTask DisposeAsync()
    {
        for (var i = _hosts.Count - 1; i >= 0; i--)
        {
            try { await _hosts[i].DisposeAsync().ConfigureAwait(false); }
            catch { /* ignore */ }
        }

        _hosts.Clear();
    }
}
