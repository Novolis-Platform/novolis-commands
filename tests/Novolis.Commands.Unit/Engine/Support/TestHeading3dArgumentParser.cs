using System.Globalization;
using Novolis.Commands.Engine;

namespace Novolis.Commands.Engine.Tests.Support;

/// <summary>
/// Test double for Bridge Commander 3D heading grammar (engine no longer ships domain parsers).
/// </summary>
internal sealed class TestHeading3dArgumentParser : ICommandArgumentParser
{
    private static readonly HashSet<string> Fillers = new(StringComparer.OrdinalIgnoreCase)
    {
        "to", "at", "the", "degrees", "degree", "set", "heading", "course"
    };

    public bool TryParse(
        CommandDefinition definition,
        IReadOnlyList<string> argumentTokens,
        out IReadOnlyDictionary<string, object?> arguments,
        out ParseFailure? failure)
    {
        arguments = new Dictionary<string, object?>();
        failure = null;

        var tokens = argumentTokens
            .Where(t => !Fillers.Contains(t))
            .ToArray();

        if (tokens.Length == 0)
        {
            failure = new ParseFailure(
                ParseFailureCode.MissingArgument,
                "Missing required argument 'heading'.",
                "heading");
            return false;
        }

        var index = 0;
        if (!TryReadAxis(tokens, ref index, out var heading))
        {
            failure = new ParseFailure(
                ParseFailureCode.InvalidArgument,
                "Invalid heading.",
                string.Join(" ", argumentTokens));
            return false;
        }

        double? headingBy = null;
        if (index < tokens.Length &&
            string.Equals(tokens[index], "by", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            if (!TryReadAxis(tokens, ref index, out var byValue))
            {
                failure = new ParseFailure(
                    ParseFailureCode.InvalidArgument,
                    "Invalid heading.",
                    string.Join(" ", argumentTokens));
                return false;
            }

            headingBy = byValue;
        }

        if (index != tokens.Length)
        {
            failure = new ParseFailure(
                ParseFailureCode.InvalidArgument,
                "Unexpected extra arguments.",
                tokens[index]);
            return false;
        }

        var args = new Dictionary<string, object?> { ["heading"] = heading };
        if (headingBy is double by)
            args["headingBy"] = by;

        arguments = args;
        return true;
    }

    private static bool TryReadAxis(string[] tokens, ref int index, out double value)
    {
        value = 0;
        if (index >= tokens.Length)
            return false;

        var token = tokens[index];
        if (!TryParseToken(token, out value))
            return false;

        index++;

        if (HasEmbeddedFraction(token))
            return true;

        if (index < tokens.Length &&
            string.Equals(tokens[index], "mark", StringComparison.OrdinalIgnoreCase))
        {
            index++;
            if (index >= tokens.Length || !int.TryParse(tokens[index], out var markDigit))
                return false;

            index++;
            var whole = Math.Truncate(value);
            value = whole + markDigit / 10.0;
        }

        return true;
    }

    private static bool TryParseToken(string token, out double value)
    {
        value = 0;
        if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
        {
            value = integer;
            return true;
        }

        var normalized = token.Replace(',', '.');
        if (!double.TryParse(
                normalized,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out value))
            return false;

        return double.IsFinite(value);
    }

    private static bool HasEmbeddedFraction(string token) =>
        token.Contains(',') || token.Contains('.');
}
