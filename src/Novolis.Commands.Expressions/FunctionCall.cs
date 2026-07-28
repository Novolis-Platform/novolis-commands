namespace Novolis.Commands.Expressions;

/// <summary>Parsed function-call expression: <c>Name(arg, …)</c> or bare <c>Name</c>.</summary>
/// <param name="Name">Command / function identifier (case preserved from input).</param>
/// <param name="Arguments">Positional arguments; empty when the call has no parentheses or empty <c>()</c>.</param>
/// <param name="OriginalPrompt">Trimmed original prompt text.</param>
/// <param name="HasParentheses">True when the prompt included a parenthesized argument list.</param>
public sealed record FunctionCall(
    string Name,
    IReadOnlyList<ExpressionArg> Arguments,
    string OriginalPrompt,
    bool HasParentheses);
