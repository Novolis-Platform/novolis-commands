namespace Novolis.Commands.Expressions;

/// <summary>A single argument from a function-call expression.</summary>
/// <param name="Raw">Original text of the argument token.</param>
/// <param name="Number">Parsed number when the argument is numeric; otherwise null.</param>
/// <param name="Text">Unquoted identifier or quoted string content when not purely numeric.</param>
public readonly record struct ExpressionArg(string Raw, double? Number, string? Text)
{
    /// <summary>True when <see cref="Number"/> was successfully parsed.</summary>
    public bool IsNumber => Number is not null;

    /// <summary>Creates a numeric argument.</summary>
    public static ExpressionArg FromNumber(double value, string? raw = null) =>
        new(raw ?? value.ToString(System.Globalization.CultureInfo.InvariantCulture), value, null);

    /// <summary>Creates a text argument.</summary>
    public static ExpressionArg FromText(string text) =>
        new(text, null, text);
}
