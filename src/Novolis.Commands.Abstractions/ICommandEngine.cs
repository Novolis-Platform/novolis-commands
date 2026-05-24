namespace Novolis.Commands;

/// <summary>Parses user prompts into <see cref="CommandEnvelope"/> instances.</summary>
/// <typeparam name="TContext">Execution context passed to parsers and processors.</typeparam>
public interface ICommandEngine<TContext>
{
    /// <summary>Parses a prompt into a command or structured failure.</summary>
    /// <param name="prompt">Raw user input.</param>
    /// <param name="context">Current command context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success with envelope, or failures and suggestions.</returns>
    ValueTask<ParseResult> ParseCommandAsync(
        string prompt,
        TContext context,
        CancellationToken cancellationToken = default);
}
