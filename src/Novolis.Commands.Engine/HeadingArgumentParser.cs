namespace Novolis.Commands.Engine;

/// <summary>
/// Parses helm heading arguments: primary axis, optional MARK decimal, optional BY second axis (also MARK).
/// Examples: 270 | 122 by 180 | 122 mark 6 by 180 mark 2
/// </summary>
public static class HeadingArgumentParser
{
    private static readonly HashSet<string> Fillers = new(StringComparer.OrdinalIgnoreCase)
    {
        "to", "at", "the", "degrees", "degree", "set", "heading"
    };

    public static bool TryParse(
        IReadOnlyList<string> argumentTokens,
        out double heading,
        out double? headingBy)
    {
        heading = 0;
        headingBy = null;

        var tokens = argumentTokens
            .Where(t => !Fillers.Contains(t))
            .ToArray();

        if (tokens.Length == 0)
            return false;

        var index = 0;
        if (!TryReadAxis(tokens, ref index, out heading))
            return false;

        if (index < tokens.Length &&
            string.Equals(tokens[index], "by", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            if (!TryReadAxis(tokens, ref index, out var byValue))
                return false;

            headingBy = byValue;
        }

        return index == tokens.Length;
    }

    private static bool TryReadAxis(string[] tokens, ref int index, out double value)
    {
        value = 0;
        if (!TryReadNumber(tokens, ref index, out var whole))
            return false;

        value = whole;

        if (index < tokens.Length &&
            string.Equals(tokens[index], "mark", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            if (index >= tokens.Length || !TryReadNumber(tokens, ref index, out var markDigit))
                return false;

            value = whole + markDigit / 10.0;
        }

        return true;
    }

    private static bool TryReadNumber(string[] tokens, ref int index, out int value)
    {
        value = 0;
        if (index >= tokens.Length)
            return false;

        if (!int.TryParse(tokens[index], out value))
            return false;

        index++;
        return true;
    }
}
