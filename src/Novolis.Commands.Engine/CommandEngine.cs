namespace Novolis.Commands.Engine;

public sealed class CommandEngine<TContext>(
    ICommandRegistry registry,
    ICommandContextResolver<TContext> contextResolver,
    BuiltInCommandMatcher? builtInMatcher = null) : ICommandEngine<TContext>
{
    private readonly BuiltInCommandMatcher _builtInMatcher = builtInMatcher ?? new BuiltInCommandMatcher();
    private readonly CommandParser _parser = new(registry);

    public ValueTask<ParseResult> ParseCommandAsync(
        string prompt,
        TContext context,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(prompt))
        {
            return ValueTask.FromResult(ParseResult.Failed(
                new ParseFailure(ParseFailureCode.EmptyPrompt, "Prompt is empty.")));
        }

        var trimmed = prompt.Trim();
        var normalized = trimmed.ToLowerInvariant();

        if (_builtInMatcher.TryMatch(normalized, out var builtIn))
        {
            return ValueTask.FromResult(ParseResult.Succeeded(builtIn!));
        }

        var tokens = normalized.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var aliases = contextResolver.GetAliases(context);
        var activeContext = contextResolver.GetActiveContextWord(context);

        string? explicitContext = null;
        var verbStartIndex = 0;

        if (tokens.Length > 0 && IsKnownContext(tokens[0]))
        {
            explicitContext = tokens[0];
            verbStartIndex = 1;
        }

        if (verbStartIndex >= tokens.Length)
        {
            return ValueTask.FromResult(ParseResult.Failed(
                new ParseFailure(ParseFailureCode.UnknownCommand, "Unknown command.", trimmed)));
        }

        var result = _parser.Parse(
            trimmed,
            normalized,
            tokens,
            explicitContext,
            activeContext,
            aliases);

        return ValueTask.FromResult(result);
    }

    private bool IsKnownContext(string token) =>
        registry.GetAll().Any(d =>
            d.ContextWord is not null &&
            string.Equals(d.ContextWord, token, StringComparison.OrdinalIgnoreCase));
}
