using System.Buffers;

namespace Novolis.Commands.Engine;

/// <summary>
/// Splits normalized command prompts into tokens using .NET whitespace rules,
/// then trims leading/trailing punctuation from each token.
/// </summary>
public static class CommandTokenizer
{
    private static readonly SearchValues<char> Whitespace = SearchValues.Create(" \t\r\n\v\f");
    private static readonly SearchValues<char> Punctuation = SearchValues.Create(",;:.!?");

    /// <summary>Tokenizes a normalized prompt string.</summary>
    /// <param name="normalized">Lowercased, trimmed prompt.</param>
    /// <returns>Non-empty tokens with punctuation trimmed.</returns>
    public static string[] Tokenize(string normalized)
    {
        if (normalized.Length == 0)
            return [];

        var span = normalized.AsSpan();
        var count = CountTokens(span);
        if (count == 0)
            return [];

        var tokens = new string[count];
        var index = 0;
        foreach (var range in span.SplitAny(Whitespace))
        {
            if (range.Start.Value == range.End.Value)
                continue;

            var token = TrimPunctuation(span[range]);
            if (token.Length == 0)
                continue;

            tokens[index++] = token.ToString();
        }

        return index == tokens.Length ? tokens : tokens[..index];
    }

    private static ReadOnlySpan<char> TrimPunctuation(ReadOnlySpan<char> token)
    {
        var start = 0;
        var end = token.Length;

        while (start < end && Punctuation.Contains(token[start]))
            start++;

        while (end > start && Punctuation.Contains(token[end - 1]))
            end--;

        return token[start..end];
    }

    private static int CountTokens(ReadOnlySpan<char> span)
    {
        var count = 0;
        foreach (var range in span.SplitAny(Whitespace))
        {
            if (range.Start.Value == range.End.Value)
                continue;

            if (TrimPunctuation(span[range]).Length > 0)
                count++;
        }

        return count;
    }
}
