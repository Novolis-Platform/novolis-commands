namespace Novolis.Commands.Engine;

/// <summary>Default <see cref="ICommandEngine{TContext}"/> implementation.</summary>
/// <typeparam name="TContext">Command execution context.</typeparam>
public sealed class CommandEngine<TContext> : ICommandEngine<TContext>
{
    private readonly ICommandRegistry _registry;
    private readonly ICommandContextResolver<TContext> _contextResolver;
    private readonly BuiltInCommandMatcher _builtInMatcher;
    private readonly CommandParser _parser;

    /// <summary>Creates an engine with optional argument parsers and built-in matcher.</summary>
    public CommandEngine(
        ICommandRegistry registry,
        ICommandContextResolver<TContext> contextResolver,
        CommandEngineOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(contextResolver);

        var engineOptions = options ?? new CommandEngineOptions();
        var parsers = engineOptions.ArgumentParsers.ToFrozen();
        CommandRegistryValidator.ValidateArgumentParserKeys(registry.GetAll(), parsers.Keys);

        _registry = registry;
        _contextResolver = contextResolver;
        _builtInMatcher = engineOptions.BuiltInMatcher ?? new BuiltInCommandMatcher();
        _parser = new CommandParser(registry, parsers);
    }

    /// <summary>Creates an engine with an explicit built-in command matcher.</summary>
    public CommandEngine(
        ICommandRegistry registry,
        ICommandContextResolver<TContext> contextResolver,
        BuiltInCommandMatcher builtInMatcher)
        : this(registry, contextResolver, new CommandEngineOptions { BuiltInMatcher = builtInMatcher })
    {
    }

    /// <inheritdoc />
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

        var contextAliases = _contextResolver.GetContextAliases(context);
        var explicitContext = ResolveContextWord(tokens[0], contextAliases);

        if (explicitContext is null)
        {
            return ValueTask.FromResult(ParseResult.Failed(
                new ParseFailure(
                    ParseFailureCode.UnknownContext,
                    "Orders must start with a station prefix (e.g. helm, tactical, weaps, pilot).",
                    tokens[0])));
        }

        var verbAliases = _contextResolver.GetVerbAliases(context);
        var result = _parser.Parse(trimmed, normalized, tokens, explicitContext, verbAliases);
        return ValueTask.FromResult(result);
    }

    private string? ResolveContextWord(string firstToken, IReadOnlyDictionary<string, string> contextAliases)
    {
        if (contextAliases.TryGetValue(firstToken, out var alias))
            return alias;

        if (_registry.IsKnownContext(firstToken))
            return firstToken;

        return null;
    }
}
