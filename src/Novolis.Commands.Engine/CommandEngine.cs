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
            return ValueTask.FromResult(ParseResult.Succeeded(builtIn!));

        var tokens = CommandTokenizer.Tokenize(normalized);
        if (tokens.Length == 0)
        {
            return ValueTask.FromResult(ParseResult.Failed(
                new ParseFailure(ParseFailureCode.UnknownCommand, "Unknown command.", trimmed)));
        }

        var contextAliases = contextResolver.GetContextAliases(context);
        var explicitContext = ResolveContextWord(tokens[0], contextAliases);

        if (explicitContext is null)
        {
            return ValueTask.FromResult(ParseResult.Failed(
                new ParseFailure(
                    ParseFailureCode.UnknownContext,
                    "Orders must start with a station prefix (e.g. helm, tactical, weaps, pilot).",
                    tokens[0])));
        }

        var verbAliases = contextResolver.GetVerbAliases(context);
        var result = _parser.Parse(trimmed, normalized, tokens, explicitContext, verbAliases);
        return ValueTask.FromResult(result);
    }

    private string? ResolveContextWord(string firstToken, IReadOnlyDictionary<string, string> contextAliases)
    {
        if (contextAliases.TryGetValue(firstToken, out var alias))
            return alias;

        if (IsKnownContext(firstToken))
            return firstToken;

        return null;
    }

    private bool IsKnownContext(string token) => registry.IsKnownContext(token);
}
