using System.Threading.Channels;

namespace Novolis.Commands.Testing;

public sealed class RecordingCommandQueue : ICommandQueue
{
    private readonly Channel<CommandEnvelope> _channel =
        Channel.CreateUnbounded<CommandEnvelope>();

    private readonly List<CommandEnvelope> _enqueued = [];

    public IReadOnlyList<CommandEnvelope> Enqueued => _enqueued;

    public async ValueTask EnqueueAsync(
        CommandEnvelope command,
        CancellationToken cancellationToken = default)
    {
        _enqueued.Add(command);
        await _channel.Writer.WriteAsync(command, cancellationToken);
    }

    public IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask ClearPendingAsync(CancellationToken cancellationToken = default)
    {
        while (_channel.Reader.TryRead(out _))
        {
        }

        return ValueTask.CompletedTask;
    }
}
