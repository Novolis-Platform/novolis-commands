namespace Novolis.Agent.Session;

/// <summary>No-op transport placeholder for future Tcp/Http hosts (protocol unchanged).</summary>
public sealed class NoopSessionTransport : ISessionTransport
{
    public string Kind { get; }

    public NoopSessionTransport(string kind = "noop") => Kind = kind;

    public ValueTask StartAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

    public ValueTask StopAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}
