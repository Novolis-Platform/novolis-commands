using System.Threading.Channels;

namespace Novolis.Commands.Queueing;

/// <summary>Unbounded channel-backed <see cref="ICommandQueue"/>.</summary>
public sealed class ChannelCommandQueue : ICommandQueue
{
    private readonly Channel<CommandEnvelope> _channel =
        Channel.CreateUnbounded<CommandEnvelope>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

    /// <inheritdoc />
    public async ValueTask EnqueueAsync(
        CommandEnvelope command,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(command, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);

    /// <inheritdoc />
    public ValueTask ClearPendingAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        while (_channel.Reader.TryRead(out _))
        {
        }

        return ValueTask.CompletedTask;
    }
}
