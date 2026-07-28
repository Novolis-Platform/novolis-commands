using System.Globalization;
using System.Text;

namespace Novolis.Commands.Expressions;

/// <summary>Parses function-call style prompts such as <c>Line(0,1)</c> or bare <c>Undo</c>.</summary>
public static class FunctionCallParser
{
    /// <summary>Attempts to parse <paramref name="prompt"/> into a <see cref="FunctionCall"/>.</summary>
    public static FunctionCallParseResult TryParse(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return FunctionCallParseResult.Failed(FunctionCallParseError.Empty, "Prompt is empty.");

        var text = prompt.Trim();
        var i = 0;
        if (!TryReadIdentifier(text, ref i, out var name) || name.Length == 0)
            return FunctionCallParseResult.Failed(FunctionCallParseError.InvalidName, "Expected a command name.");

        SkipWhitespace(text, ref i);
        if (i >= text.Length)
        {
            return FunctionCallParseResult.Succeeded(
                new FunctionCall(name, [], text, HasParentheses: false));
        }

        if (text[i] != '(')
            return FunctionCallParseResult.Failed(
                FunctionCallParseError.TrailingText,
                $"Unexpected text after '{name}'.");

        i++;
        var args = new List<ExpressionArg>();
        SkipWhitespace(text, ref i);

        if (i < text.Length && text[i] == ')')
        {
            i++;
            SkipWhitespace(text, ref i);
            if (i < text.Length)
                return FunctionCallParseResult.Failed(FunctionCallParseError.TrailingText, "Trailing text after call.");

            return FunctionCallParseResult.Succeeded(
                new FunctionCall(name, args, text, HasParentheses: true));
        }

        while (i < text.Length)
        {
            SkipWhitespace(text, ref i);
            if (i >= text.Length)
                return FunctionCallParseResult.Failed(
                    FunctionCallParseError.UnbalancedParentheses,
                    "Missing closing ')'.");

            if (!TryReadArgument(text, ref i, out var arg, out var argError))
                return FunctionCallParseResult.Failed(argError, $"Invalid argument near position {i}.");

            args.Add(arg);
            SkipWhitespace(text, ref i);
            if (i >= text.Length)
                return FunctionCallParseResult.Failed(
                    FunctionCallParseError.UnbalancedParentheses,
                    "Missing closing ')'.");

            if (text[i] == ',')
            {
                i++;
                SkipWhitespace(text, ref i);
                if (i < text.Length && text[i] == ')')
                    return FunctionCallParseResult.Failed(
                        FunctionCallParseError.InvalidArgument,
                        "Trailing comma in argument list.");
                continue;
            }

            if (text[i] == ')')
            {
                i++;
                SkipWhitespace(text, ref i);
                if (i < text.Length)
                    return FunctionCallParseResult.Failed(
                        FunctionCallParseError.TrailingText,
                        "Trailing text after call.");

                return FunctionCallParseResult.Succeeded(
                    new FunctionCall(name, args, text, HasParentheses: true));
            }

            return FunctionCallParseResult.Failed(
                FunctionCallParseError.InvalidArgument,
                $"Expected ',' or ')' near position {i}.");
        }

        return FunctionCallParseResult.Failed(
            FunctionCallParseError.UnbalancedParentheses,
            "Missing closing ')'.");
    }

    private static bool TryReadIdentifier(string text, ref int i, out string name)
    {
        var start = i;
        if (i >= text.Length || !IsIdentStart(text[i]))
        {
            name = string.Empty;
            return false;
        }

        i++;
        while (i < text.Length && IsIdentPart(text[i]))
            i++;

        name = text[start..i];
        return true;
    }

    private static bool TryReadArgument(
        string text,
        ref int i,
        out ExpressionArg arg,
        out FunctionCallParseError error)
    {
        arg = default;
        error = FunctionCallParseError.InvalidArgument;

        if (i >= text.Length)
            return false;

        if (text[i] is '"' or '\'')
        {
            var quote = text[i++];
            var sb = new StringBuilder();
            while (i < text.Length && text[i] != quote)
            {
                if (text[i] == '\\' && i + 1 < text.Length)
                {
                    i++;
                    sb.Append(text[i++]);
                    continue;
                }

                sb.Append(text[i++]);
            }

            if (i >= text.Length || text[i] != quote)
                return false;

            i++;
            var value = sb.ToString();
            arg = ExpressionArg.FromText(value);
            return true;
        }

        var start = i;
        while (i < text.Length && text[i] is not (',' or ')'))
            i++;

        var raw = text[start..i].Trim();
        if (raw.Length == 0)
            return false;

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            arg = ExpressionArg.FromNumber(number, raw);
            return true;
        }

        arg = ExpressionArg.FromText(raw);
        return true;
    }

    private static void SkipWhitespace(string text, ref int i)
    {
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;
    }

    private static bool IsIdentStart(char c) =>
        char.IsLetter(c) || c == '_';

    private static bool IsIdentPart(char c) =>
        char.IsLetterOrDigit(c) || c == '_';
}
