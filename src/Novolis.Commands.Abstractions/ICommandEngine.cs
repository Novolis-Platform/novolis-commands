namespace Novolis.Commands;

public interface ICommandEngine<TContext>
{
    ValueTask<ParseResult> ParseCommandAsync(
        string prompt,
        TContext context,
        CancellationToken cancellationToken = default);
}
