using System.Globalization;
using System.Text;

namespace Novolis.Commands.Expressions;

/// <summary>
/// Parses function-call style prompts such as <c>Line(0,1,2,3)</c>, nested
/// <c>Line(Point(0,1), Point(1,1))</c>, scripts <c>Line(...); Circle(...);</c>, or bare <c>Undo</c>.
/// </summary>
public static class FunctionCallParser
{
    /// <summary>Attempts to parse a single call from <paramref name="prompt"/>.</summary>
    public static FunctionCallParseResult TryParse(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return FunctionCallParseResult.Failed(FunctionCallParseError.Empty, "Prompt is empty.");

        var text = prompt.Trim();
        var i = 0;
        if (!TryParseCallAt(text, ref i, out var call, out var error, out var message))
            return FunctionCallParseResult.Failed(error, message);

        SkipWhitespaceAndSemicolons(text, ref i);
        if (i < text.Length)
            return FunctionCallParseResult.Failed(FunctionCallParseError.TrailingText, "Trailing text after call.");

        return FunctionCallParseResult.Succeeded(call);
    }

    /// <summary>
    /// Parses one or more calls separated by <c>;</c>
    /// (AutoCAD-style: <c>Func(a); Func(b);</c>).
    /// </summary>
    public static FunctionCallScriptParseResult TryParseScript(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return FunctionCallScriptParseResult.Failed(FunctionCallParseError.Empty, "Prompt is empty.");

        var text = prompt.Trim();
        var i = 0;
        var calls = new List<FunctionCall>();
        while (i < text.Length)
        {
            SkipWhitespaceAndSemicolons(text, ref i);
            if (i >= text.Length)
                break;

            if (!TryParseCallAt(text, ref i, out var call, out var error, out var message))
                return FunctionCallScriptParseResult.Failed(error, message);

            calls.Add(call);
            SkipWhitespace(text, ref i);
            if (i < text.Length && text[i] == ';')
            {
                i++;
                continue;
            }

            SkipWhitespace(text, ref i);
            if (i < text.Length)
                return FunctionCallScriptParseResult.Failed(
                    FunctionCallParseError.TrailingText,
                    $"Expected ';' between calls near position {i}.");
        }

        if (calls.Count == 0)
            return FunctionCallScriptParseResult.Failed(FunctionCallParseError.Empty, "Prompt is empty.");

        return FunctionCallScriptParseResult.Succeeded(calls);
    }

    /// <summary>Parses one call starting at <paramref name="i"/>; leaves <paramref name="i"/> after the call (before optional <c>;</c>).</summary>
    public static bool TryParseCallAt(
        string text,
        ref int i,
        out FunctionCall call,
        out FunctionCallParseError error,
        out string message)
    {
        call = null!;
        error = FunctionCallParseError.InvalidName;
        message = "Expected a command name.";

        SkipWhitespace(text, ref i);
        var start = i;
        if (!TryReadIdentifier(text, ref i, out var name) || name.Length == 0)
            return false;

        SkipWhitespace(text, ref i);
        if (i >= text.Length || text[i] != '(')
        {
            call = new FunctionCall(name, [], text[start..i].TrimEnd(), HasParentheses: false);
            error = default;
            message = string.Empty;
            return true;
        }

        i++;
        var args = new List<ExpressionArg>();
        SkipWhitespace(text, ref i);

        if (i < text.Length && text[i] == ')')
        {
            i++;
            call = new FunctionCall(name, args, text[start..i], HasParentheses: true);
            error = default;
            message = string.Empty;
            return true;
        }

        while (i < text.Length)
        {
            SkipWhitespace(text, ref i);
            if (i >= text.Length)
            {
                error = FunctionCallParseError.UnbalancedParentheses;
                message = "Missing closing ')'.";
                return false;
            }

            if (!TryReadArgument(text, ref i, out var arg, out error))
            {
                message = $"Invalid argument near position {i}.";
                return false;
            }

            args.Add(arg);
            SkipWhitespace(text, ref i);
            if (i >= text.Length)
            {
                error = FunctionCallParseError.UnbalancedParentheses;
                message = "Missing closing ')'.";
                return false;
            }

            if (text[i] == ',')
            {
                i++;
                SkipWhitespace(text, ref i);
                if (i < text.Length && text[i] == ')')
                {
                    error = FunctionCallParseError.InvalidArgument;
                    message = "Trailing comma in argument list.";
                    return false;
                }

                continue;
            }

            if (text[i] == ')')
            {
                i++;
                call = new FunctionCall(name, args, text[start..i], HasParentheses: true);
                error = default;
                message = string.Empty;
                return true;
            }

            error = FunctionCallParseError.InvalidArgument;
            message = $"Expected ',' or ')' near position {i}.";
            return false;
        }

        error = FunctionCallParseError.UnbalancedParentheses;
        message = "Missing closing ')'.";
        return false;
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
            arg = ExpressionArg.FromText(sb.ToString());
            return true;
        }

        // Nested call: Ident(...)
        var save = i;
        if (TryReadIdentifier(text, ref i, out _))
        {
            SkipWhitespace(text, ref i);
            if (i < text.Length && text[i] == '(')
            {
                i = save;
                if (!TryParseCallAt(text, ref i, out var nested, out error, out _))
                    return false;
                arg = ExpressionArg.FromCall(nested);
                return true;
            }

            i = save;
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

    private static void SkipWhitespaceAndSemicolons(string text, ref int i)
    {
        while (i < text.Length && (char.IsWhiteSpace(text[i]) || text[i] == ';'))
            i++;
    }

    private static bool IsIdentStart(char c) =>
        char.IsLetter(c) || c == '_';

    private static bool IsIdentPart(char c) =>
        char.IsLetterOrDigit(c) || c == '_';
}
