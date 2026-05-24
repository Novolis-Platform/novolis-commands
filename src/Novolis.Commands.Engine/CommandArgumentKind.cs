namespace Novolis.Commands.Engine;

/// <summary>Primitive argument types supported by the default parser.</summary>
public enum CommandArgumentKind
{
    /// <summary>32-bit signed integer token.</summary>
    Integer,

    /// <summary>String token (greedy for the last argument).</summary>
    String,

    /// <summary>Floating-point token.</summary>
    Double
}
