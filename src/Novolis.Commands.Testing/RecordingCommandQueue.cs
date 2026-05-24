using System.Threading.Channels;

namespace Novolis.Commands.Testing;

/// <summary>Queue that records enqueued commands for assertions.</summary>
public sealed class RecordingCommandQueue : ICommandQueue
{
    private readonly Channel<CommandEnvelope> _channel =
        Channel.CreateUnbounded<CommandEnvelope>();

    private readonly List<CommandEnvelope> _enqueued = [];

    /// <summary>Commands passed to <see cref="EnqueueAsync"/>.</summary>
    public IReadOnlyList<CommandEnvelope> Enqueued => _enqueued;

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(
        CommandEnvelope command,
        CancellationToken cancellationToken = default)
    {
        _enqueued.Add(command);
        await _channel.Writer.WriteAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);

    /// <inheritdoc />
    public ValueTask ClearPendingAsync(CancellationToken cancellationToken = default)
    {
        while (_channel.Reader.TryRead(out _))
        {
        }

        return ValueTask.CompletedTask;
    }
}
