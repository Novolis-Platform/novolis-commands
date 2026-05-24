namespace Novolis.Commands;

/// <summary>Represents ICommandEngine<TContext>.</summary>
public interface ICommandEngine<TContext>
/// <summary>ParseCommandAsync operation.</summary>
{
    ValueTask<ParseResult> ParseCommandAsync(
        string prompt,
        TContext context,
        CancellationToken cancellationToken = default);
}
