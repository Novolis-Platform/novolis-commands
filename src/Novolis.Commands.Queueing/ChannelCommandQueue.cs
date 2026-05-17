using System.Threading.Channels;

namespace Novolis.Commands.Queueing;

public sealed class ChannelCommandQueue : ICommandQueue
{
    private readonly Channel<CommandEnvelope> _channel =
        Channel.CreateUnbounded<CommandEnvelope>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

    public async ValueTask EnqueueAsync(
        CommandEnvelope command,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public IAsyncEnumerable<CommandEnvelope> ReadAllAsync(
        CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}
