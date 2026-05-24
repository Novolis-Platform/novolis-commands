namespace Novolis.Commands;

/// <summary>Represents ParseFailureCode.</summary>
public enum ParseFailureCode
/// <summary>EmptyPrompt.</summary>
{
    /// <summary>UnknownCommand.</summary>
    EmptyPrompt,
    /// <summary>InvalidArgument.</summary>
    UnknownContext,
    UnknownCommand,
    MissingArgument,
    InvalidArgument,
    AmbiguousCommand
}
