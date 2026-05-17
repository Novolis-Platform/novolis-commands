namespace Novolis.Commands.Engine;

public sealed class CommandParser(ICommandRegistry registry)
{
    public ParseResult Parse(
        string originalPrompt,
        string normalizedPrompt,
        IReadOnlyList<string> tokens,
        string? explicitContextWord,
        string? activeContextWord,
        IReadOnlyDictionary<string, string> aliases)
    {
        if (tokens.Count == 0)
        {
            return ParseResult.Failed(
                new ParseFailure(ParseFailureCode.UnknownCommand, "No command verb found.", normalizedPrompt));
        }

        var contextWord = explicitContextWord ?? activeContextWord;
        var verbStartIndex = explicitContextWord is not null ? 1 : 0;

        if (verbStartIndex >= tokens.Count)
        {
            return ParseResult.Failed(
                new ParseFailure(ParseFailureCode.UnknownCommand, "No command verb found.", normalizedPrompt));
        }

        var verb = tokens[verbStartIndex];
        var resolvedVerb = ResolveAlias(verb, aliases);
        var matches = FindMatches(resolvedVerb, contextWord, activeContextWord);

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

            return ParseResult.Failed(
                new ParseFailure(ParseFailureCode.UnknownCommand, "Unknown command.", verb));
        }

        if (matches.Count > 1)
        {
            return BuildAmbiguousResult(verb, matches, activeContextWord);
        }

        var definition = matches[0];
        var argumentTokens = tokens.Skip(verbStartIndex + 1).ToArray();
        return BuildSuccess(originalPrompt, definition, argumentTokens, contextWord ?? definition.ContextWord);
    }

    private ParseResult BuildSuccess(
        string originalPrompt,
        CommandDefinition definition,
        string[] argumentTokens,
        string? contextWord)
    {
        var arguments = new Dictionary<string, object?>();
        var argIndex = 0;

        foreach (var argDef in definition.Arguments)
        {
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
            default:
                value = null;
                return false;
        }
    }

    private ParseResult BuildAmbiguousResult(
        string verb,
        IReadOnlyList<CommandDefinition> matches,
        string? activeContextWord)
    {
        var candidates = matches
            .Select(m => new CommandCandidate(
                m.Name,
                m.ContextWord,
                new Dictionary<string, object?>(),
                ScoreMatch(m, activeContextWord),
                DescribeMatch(m, activeContextWord)))
            .OrderByDescending(c => c.Confidence)
            .ToArray();

        return new ParseResult
        {
            Success = false,
            Failures =
            [
                new ParseFailure(ParseFailureCode.AmbiguousCommand, "Command is ambiguous.", verb)
            ],
            Candidates = candidates
        };
    }

    private static double ScoreMatch(CommandDefinition definition, string? activeContextWord)
    {
        var score = 0.5;
        if (definition.ContextWord is not null &&
            string.Equals(definition.ContextWord, activeContextWord, StringComparison.OrdinalIgnoreCase))
        {
            score += 0.12;
        }

        return Math.Min(score, 0.99);
    }

    private static string DescribeMatch(CommandDefinition definition, string? activeContextWord) =>
        definition.ContextWord is not null &&
        string.Equals(definition.ContextWord, activeContextWord, StringComparison.OrdinalIgnoreCase)
            ? "Matches active context."
            : $"Matches {definition.ContextWord ?? "global"} verb.";

    private List<CommandDefinition> FindMatches(string verb, string? contextWord, string? activeContextWord)
    {
        var results = new List<CommandDefinition>();

        foreach (var definition in registry.GetAll())
        {
            if (!definition.Verbs.Any(v => string.Equals(v, verb, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (contextWord is not null)
            {
                if (definition.ContextWord is null ||
                    !string.Equals(definition.ContextWord, contextWord, StringComparison.OrdinalIgnoreCase))
                    continue;
            }
            else if (definition.ContextWord is not null &&
                     activeContextWord is not null &&
                     !string.Equals(definition.ContextWord, activeContextWord, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            results.Add(definition);
        }

        if (results.Count > 1 && contextWord is null)
            return results;

        if (results.Count == 0 && contextWord is null)
        {
            foreach (var definition in registry.GetAll())
            {
                if (definition.Verbs.Any(v => string.Equals(v, verb, StringComparison.OrdinalIgnoreCase)))
                    results.Add(definition);
            }
        }

        return results;
    }

    private bool HasContext(string contextWord) =>
        registry.GetAll().Any(d =>
            d.ContextWord is not null &&
            string.Equals(d.ContextWord, contextWord, StringComparison.OrdinalIgnoreCase));

    private static string ResolveAlias(string token, IReadOnlyDictionary<string, string> aliases) =>
        aliases.TryGetValue(token, out var resolved) ? resolved : token;
}
