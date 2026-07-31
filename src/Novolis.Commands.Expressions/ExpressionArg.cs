namespace Novolis.Commands.Expressions;

/// <summary>A single argument from a function-call expression.</summary>
/// <param name="Raw">Original text of the argument token.</param>
/// <param name="Number">Parsed number when the argument is numeric; otherwise null.</param>
/// <param name="Text">Unquoted identifier or quoted string content when not purely numeric.</param>
/// <param name="Call">Nested function call when the argument is itself a call (e.g. <c>Point(0,1)</c>).</param>
public readonly record struct ExpressionArg(string Raw, double? Number, string? Text, FunctionCall? Call = null)
{
    /// <summary>True when <see cref="Number"/> was successfully parsed.</summary>
    public bool IsNumber => Number is not null;

    /// <summary>True when <see cref="Call"/> is present.</summary>
    public bool IsCall => Call is not null;

    /// <summary>Creates a numeric argument.</summary>
    public static ExpressionArg FromNumber(double value, string? raw = null) =>
        new(raw ?? value.ToString(System.Globalization.CultureInfo.InvariantCulture), value, null);

    /// <summary>Creates a text argument.</summary>
    public static ExpressionArg FromText(string text) =>
        new(text, null, text);

    /// <summary>Creates a nested-call argument.</summary>
    public static ExpressionArg FromCall(FunctionCall call) =>
        new(call.OriginalPrompt, null, null, call);
}
