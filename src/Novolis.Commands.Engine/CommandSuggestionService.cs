using System.Collections.Frozen;

namespace Novolis.Commands.Engine;

/// <summary>
/// Suggests close verb phrases when parsing fails with <see cref="ParseFailureCode.UnknownCommand"/>.
/// </summary>
public sealed class CommandSuggestionService
{
    /// <summary>Returns close verb phrase suggestions for a failed parse.</summary>
    public IReadOnlyList<string> Suggest(
        IReadOnlyList<string> tokens,
        int verbStartIndex,
        string? contextWord,
        ICommandRegistry registry,
        int maxSuggestions = 3)
    {
        if (verbStartIndex >= tokens.Count)
            return [];

        var typedFragment = string.Join(' ', tokens.Skip(verbStartIndex));
        var candidates = new List<(string Phrase, int Distance)>();

        foreach (var definition in registry.GetAll())
        {
            if (contextWord is not null &&
                (definition.ContextWord is null ||
                 !string.Equals(definition.ContextWord, contextWord, StringComparison.OrdinalIgnoreCase)))
                continue;

            foreach (var verb in definition.Verbs)
            {
                var distance = LevenshteinDistance(typedFragment, verb);
                candidates.Add((verb, distance));
            }
        }

        return candidates
            .OrderBy(c => c.Distance)
            .ThenBy(c => c.Phrase, StringComparer.OrdinalIgnoreCase)
            .Take(maxSuggestions)
            .Select(c => c.Phrase)
            .ToArray();
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
            return 0;

        var m = a.Length;
        var n = b.Length;
        if (m == 0)
            return n;
        if (n == 0)
            return m;

        var d = new int[m + 1, n + 1];
        for (var i = 0; i <= m; i++)
            d[i, 0] = i;
        for (var j = 0; j <= n; j++)
            d[0, j] = j;

        for (var i = 1; i <= m; i++)
        {
            for (var j = 1; j <= n; j++)
            {
                var cost = char.ToLowerInvariant(a[i - 1]) == char.ToLowerInvariant(b[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[m, n];
    }
}
