using System.Buffers;

namespace Novolis.Commands.Engine;

/// <summary>
/// Splits normalized command prompts into tokens using .NET whitespace rules
/// (<see cref="MemoryExtensions.SplitAny(ReadOnlySpan{char}, SearchValues{char})"/>).
/// </summary>
public static class CommandTokenizer
{
    private static readonly SearchValues<char> Whitespace = SearchValues.Create(" \t\r\n\v\f");

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

            tokens[index++] = span[range].ToString();
        }

        return index == tokens.Length ? tokens : tokens[..index];
    }

    private static int CountTokens(ReadOnlySpan<char> span)
    {
        var count = 0;
        foreach (var range in span.SplitAny(Whitespace))
        {
            if (range.Start.Value != range.End.Value)
                count++;
        }

        return count;
    }
}
