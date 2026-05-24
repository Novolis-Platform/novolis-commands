namespace Novolis.Commands.Engine;

/// <summary>
/// Validates command registry definitions at build or engine startup.
/// </summary>
public static class CommandRegistryValidator
{
    /// <summary>Validates command definitions and optional parser key coverage.</summary>
    public static void Validate(
        IReadOnlyList<CommandDefinition> definitions,
        IReadOnlyCollection<string>? registeredParserKeys = null,
        bool failOnCrossContextPhraseOverlap = false)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in definitions)
        {
            if (!names.Add(definition.Name))
            {
                throw new InvalidOperationException(
                    $"Duplicate command name '{definition.Name}'.");
            }

            if (definition.Verbs.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Command '{definition.Name}' has no verb phrases.");
            }

            ValidateVerbPhrases(definition);
        }

        if (registeredParserKeys is not null)
            ValidateArgumentParserKeys(definitions, registeredParserKeys);

        ValidateDuplicatePhrasesPerContext(definitions);

        if (failOnCrossContextPhraseOverlap)
            ValidateCrossContextPhraseOverlap(definitions);
    }

    /// <summary>
    /// Validates parser keys at engine startup (registry may have been built with <c>validate: false</c> for tests).
    /// </summary>
    public static void ValidateArgumentParserKeys(
        IReadOnlyList<CommandDefinition> definitions,
        IReadOnlyCollection<string> registeredParserKeys)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(registeredParserKeys);

        foreach (var definition in definitions)
        {
            if (definition.ArgumentParserKey is not null &&
                !registeredParserKeys.Contains(definition.ArgumentParserKey, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Command '{definition.Name}' references unknown argument parser key '{definition.ArgumentParserKey}'.");
            }
        }
    }

    private static void ValidateVerbPhrases(CommandDefinition definition)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var verb in definition.Verbs)
        {
            var normalized = verb.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(normalized))
            {
                throw new InvalidOperationException(
                    $"Command '{definition.Name}' has an empty verb phrase.");
            }

            if (!seen.Add(normalized))
            {
                throw new InvalidOperationException(
                    $"Command '{definition.Name}' has duplicate verb phrase '{verb}'.");
            }
        }
    }

    private static void ValidateDuplicatePhrasesPerContext(IReadOnlyList<CommandDefinition> definitions)
    {
        foreach (var group in definitions
                     .Where(d => d.ContextWord is not null)
                     .GroupBy(d => d.ContextWord!, StringComparer.OrdinalIgnoreCase))
        {
            var phraseOwners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var definition in group)
            {
                foreach (var verb in definition.Verbs)
                {
                    var phrase = verb.Trim().ToLowerInvariant();
                    if (phraseOwners.TryGetValue(phrase, out var owner) &&
                        !string.Equals(owner, definition.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Context '{definition.ContextWord}': verb phrase '{verb}' is used by both '{owner}' and '{definition.Name}'.");
                    }

                    phraseOwners[phrase] = definition.Name;
                }
            }
        }
    }

    private static void ValidateCrossContextPhraseOverlap(IReadOnlyList<CommandDefinition> definitions)
    {
        var phraseToContexts = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var context = definition.ContextWord ?? "(global)";
            foreach (var verb in definition.Verbs)
            {
                var phrase = verb.Trim().ToLowerInvariant();
                if (!phraseToContexts.TryGetValue(phrase, out var contexts))
                {
                    contexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    phraseToContexts[phrase] = contexts;
                }

                contexts.Add(context);
            }
        }

        foreach (var (phrase, contexts) in phraseToContexts)
        {
            if (contexts.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Verb phrase '{phrase}' is registered in multiple contexts: {string.Join(", ", contexts)}.");
            }
        }
    }
}
