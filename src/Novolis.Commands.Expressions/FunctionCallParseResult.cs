namespace Novolis.Commands.Expressions;

/// <summary>Success or failure of <see cref="FunctionCallParser.TryParse"/>.</summary>
public sealed class FunctionCallParseResult
{
    private FunctionCallParseResult(FunctionCall? call, FunctionCallParseError? error, string? message)
    {
        Call = call;
        Error = error;
        Message = message;
    }

    /// <summary>Parsed call when successful.</summary>
    public FunctionCall? Call { get; }

    /// <summary>Error code when unsuccessful.</summary>
    public FunctionCallParseError? Error { get; }

    /// <summary>Human-readable failure detail.</summary>
    public string? Message { get; }

    /// <summary>True when <see cref="Call"/> is present.</summary>
    public bool Success => Call is not null;

    /// <summary>Creates a successful result.</summary>
    public static FunctionCallParseResult Succeeded(FunctionCall call) =>
        new(call, null, null);

    /// <summary>Creates a failed result.</summary>
    public static FunctionCallParseResult Failed(FunctionCallParseError error, string message) =>
        new(null, error, message);
}
