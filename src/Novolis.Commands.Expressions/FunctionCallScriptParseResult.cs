namespace Novolis.Commands.Expressions;

/// <summary>Success or failure of <see cref="FunctionCallParser.TryParseScript"/>.</summary>
public sealed class FunctionCallScriptParseResult
{
    private FunctionCallScriptParseResult(
        IReadOnlyList<FunctionCall>? calls,
        FunctionCallParseError? error,
        string? message)
    {
        Calls = calls ?? Array.Empty<FunctionCall>();
        Error = error;
        Message = message;
    }

    /// <summary>Ordered calls when successful (empty only on failure).</summary>
    public IReadOnlyList<FunctionCall> Calls { get; }

    /// <summary>Error code when unsuccessful.</summary>
    public FunctionCallParseError? Error { get; }

    /// <summary>Human-readable failure detail.</summary>
    public string? Message { get; }

    /// <summary>True when at least one call was parsed.</summary>
    public bool Success => Error is null && Calls.Count > 0;

    /// <summary>Creates a successful multi-call result.</summary>
    public static FunctionCallScriptParseResult Succeeded(IReadOnlyList<FunctionCall> calls) =>
        new(calls, null, null);

    /// <summary>Creates a failed result.</summary>
    public static FunctionCallScriptParseResult Failed(FunctionCallParseError error, string message) =>
        new(null, error, message);
}
