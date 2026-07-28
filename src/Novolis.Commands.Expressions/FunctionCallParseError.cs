namespace Novolis.Commands.Expressions;

/// <summary>Why a function-call parse failed.</summary>
public enum FunctionCallParseError
{
    /// <summary>Prompt was empty or whitespace.</summary>
    Empty,

    /// <summary>Could not find a valid identifier at the start of the prompt.</summary>
    InvalidName,

    /// <summary>Opening <c>(</c> without a matching <c>)</c>.</summary>
    UnbalancedParentheses,

    /// <summary>Trailing text after a complete call.</summary>
    TrailingText,

    /// <summary>Malformed argument (e.g. empty slot, bad quote).</summary>
    InvalidArgument,
}
