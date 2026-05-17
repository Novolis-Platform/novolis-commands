using System.Collections.Frozen;

namespace Novolis.Commands.Engine;

public sealed class CommandParser
{
    private readonly ICommandRegistry _registry;
    private readonly FrozenDictionary<string, ICommandArgumentParser> _argumentParsers;
    private readonly CommandSuggestionService _suggestions = new();

    public CommandParser(
        ICommandRegistry registry,
        IReadOnlyDictionary<string, ICommandArgumentParser>? argumentParsers = null)
    {
        _registry = registry;
        _argumentParsers = argumentParsers?.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase)
            ?? FrozenDictionary<string, ICommandArgumentParser>.Empty;
    }

    public ParseResult Parse(
        string originalPrompt,
        string normalizedPrompt,
        IReadOnlyList<string> tokens,
        string? explicitContextWord,
        IReadOnlyDictionary<string, string> verbAliases)
    {
        if (tokens.Count == 0)
        {
            return ParseResult.Failed(
                new ParseFailure(ParseFailureCode.UnknownCommand, "No command verb found.", normalizedPrompt));
        }

        var contextWord = explicitContextWord;
        var verbStartIndex = explicitContextWord is not null ? 1 : 0;

        if (verbStartIndex >= tokens.Count)
        {
            return ParseResult.Failed(
                new ParseFailure(ParseFailureCode.UnknownCommand, "No command verb found.", normalizedPrompt));
        }

        var matches = FindPhraseMatches(tokens, verbStartIndex, contextWord, verbAliases);

        if (matches.Count == 0)
        {
            if (explicitContextWord is not null && !HasContext(explicitContextWord))
            {
                return ParseResult.Failed(
                    new ParseFailure(
                        ParseFailureCode.UnknownContext,
                        $"Unknown context '{explicitContextWord}'.",
                        explicitContextWord));
            }

            var suggestions = _suggestions.Suggest(tokens, verbStartIndex, contextWord, _registry);
            return ParseResult.Failed(
                [new ParseFailure(ParseFailureCode.UnknownCommand, "Unknown command.", normalizedPrompt)],
                suggestions);
        }

        var bestLength = matches.Max(m => m.PhraseLength);
        var longest = matches.Where(m => m.PhraseLength == bestLength).ToList();

        if (longest.Select(m => m.Definition.Name).Distinct().Count() > 1)
        {
            return BuildAmbiguousResult(
                string.Join(" ", tokens.Skip(verbStartIndex).Take(bestLength)),
                longest.Select(m => m.Definition).Distinct().ToList());
        }

        var match = longest[0];
        var argumentTokens = tokens.Skip(verbStartIndex + match.PhraseLength).ToArray();
        return BuildSuccess(originalPrompt, match.Definition, argumentTokens, contextWord ?? match.Definition.ContextWord);
    }

    private ParseResult BuildSuccess(
        string originalPrompt,
        CommandDefinition definition,
        string[] argumentTokens,
        string? contextWord)
    {
        if (definition.ArgumentParserKey is not null)
        {
            if (!_argumentParsers.TryGetValue(definition.ArgumentParserKey, out var parser))
            {
                return ParseResult.Failed(
                    new ParseFailure(
                        ParseFailureCode.InvalidArgument,
                        $"No argument parser registered for key '{definition.ArgumentParserKey}'.",
                        definition.ArgumentParserKey));
            }

            if (!parser.TryParse(definition, argumentTokens, out var parsedArguments, out var failure))
            {
                return failure is not null
                    ? ParseResult.Failed(failure)
                    : ParseResult.Failed(
                        new ParseFailure(
                            ParseFailureCode.InvalidArgument,
                            "Invalid arguments.",
                            string.Join(" ", argumentTokens)));
            }

            return ParseResult.Succeeded(new CommandEnvelope
            {
                Id = CommandId.New(),
                Name = definition.Name,
                OriginalPrompt = originalPrompt,
                ContextWord = contextWord,
                Arguments = parsedArguments
            });
        }

        var arguments = new Dictionary<string, object?>();
        var argDefs = definition.Arguments;
        var argIndex = 0;

        for (var i = 0; i < argDefs.Count; i++)
        {
            var argDef = argDefs[i];
            var isLast = i == argDefs.Count - 1;

            if (argIndex >= argumentTokens.Length)
            {
                if (argDef.Required)
                {
                    return ParseResult.Failed(
                        new ParseFailure(
                            ParseFailureCode.MissingArgument,
                            $"Missing required argument '{argDef.Name}'.",
                            argDef.Name));
                }

                continue;
            }

            if (argDef.Kind == CommandArgumentKind.String && isLast)
            {
                arguments[argDef.Name] = string.Join(" ", argumentTokens[argIndex..]);
                argIndex = argumentTokens.Length;
                continue;
            }

            var token = argumentTokens[argIndex++];
            if (!TryParseArgument(token, argDef, out var value))
            {
                return ParseResult.Failed(
                    new ParseFailure(
                        ParseFailureCode.InvalidArgument,
                        $"Invalid value for argument '{argDef.Name}'.",
                        token));
            }

            arguments[argDef.Name] = value;
        }

        if (argIndex < argumentTokens.Length)
        {
            return ParseResult.Failed(
                new ParseFailure(
                    ParseFailureCode.InvalidArgument,
                    "Unexpected extra arguments.",
                    argumentTokens[argIndex]));
        }

        var envelope = new CommandEnvelope
        {
            Id = CommandId.New(),
            Name = definition.Name,
            OriginalPrompt = originalPrompt,
            ContextWord = contextWord,
            Arguments = arguments
        };

        return ParseResult.Succeeded(envelope);
    }

    private static bool TryParseArgument(string token, CommandArgumentDefinition definition, out object? value)
    {
        switch (definition.Kind)
        {
            case CommandArgumentKind.Integer:
                if (int.TryParse(token, out var intValue))
                {
                    value = intValue;
                    return true;
                }

                value = null;
                return false;
            case CommandArgumentKind.String:
                value = token;
                return true;
            case CommandArgumentKind.Double:
                if (double.TryParse(token, out var doubleValue))
                {
                    value = doubleValue;
                    return true;
                }

                value = null;
                return false;
            default:
                value = null;
                return false;
        }
    }

    private ParseResult BuildAmbiguousResult(string fragment, IReadOnlyList<CommandDefinition> matches)
    {
        var candidates = matches
            .Select(m => new CommandCandidate(
                m.Name,
                m.ContextWord,
                new Dictionary<string, object?>(),
                0.5,
                $"Matches {m.ContextWord ?? "global"} phrase."))
            .OrderByDescending(c => c.Confidence)
            .ToArray();

        return new ParseResult
        {
            Success = false,
            Failures =
            [
                new ParseFailure(ParseFailureCode.AmbiguousCommand, "Command is ambiguous.", fragment)
            ],
            Candidates = candidates
        };
    }

    private List<(CommandDefinition Definition, int PhraseLength)> FindPhraseMatches(
        IReadOnlyList<string> tokens,
        int verbStartIndex,
        string? contextWord,
        IReadOnlyDictionary<string, string> verbAliases)
    {
        var matches = new List<(CommandDefinition, int)>();

        foreach (var definition in _registry.GetAll())
        {
            if (contextWord is not null &&
                (definition.ContextWord is null ||
                 !string.Equals(definition.ContextWord, contextWord, StringComparison.OrdinalIgnoreCase)))
                continue;

            foreach (var verb in definition.Verbs)
            {
                var phraseTokens = verb.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (phraseTokens.Length == 0)
                    continue;

                var resolvedPhrase = ResolveVerbPhrase(phraseTokens, verbAliases);
                if (TokensMatch(tokens, verbStartIndex, resolvedPhrase))
                    matches.Add((definition, resolvedPhrase.Length));
            }
        }

        return matches;
    }

    private static string[] ResolveVerbPhrase(string[] phraseTokens, IReadOnlyDictionary<string, string> verbAliases)
    {
        if (phraseTokens.Length == 1 && verbAliases.TryGetValue(phraseTokens[0], out var alias))
            return alias.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        return phraseTokens;
    }

    private static bool TokensMatch(IReadOnlyList<string> tokens, int startIndex, string[] phraseTokens)
    {
        if (startIndex + phraseTokens.Length > tokens.Count)
            return false;

        for (var i = 0; i < phraseTokens.Length; i++)
        {
            if (!string.Equals(tokens[startIndex + i], phraseTokens[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private bool HasContext(string contextWord) => _registry.IsKnownContext(contextWord);
}
