namespace Novolis.Commands;

/// <summary>Reasons a command prompt could not be parsed.</summary>
public enum ParseFailureCode
{
    /// <summary>Prompt was empty or whitespace.</summary>
    EmptyPrompt,

    /// <summary>Context word is not recognized.</summary>
    UnknownContext,

    /// <summary>Verb or command name is not registered.</summary>
    UnknownCommand,

    /// <summary>Required argument was not supplied.</summary>
    MissingArgument,

    /// <summary>Argument value failed validation.</summary>
    InvalidArgument,

    /// <summary>Multiple commands matched the prompt.</summary>
    AmbiguousCommand
}
