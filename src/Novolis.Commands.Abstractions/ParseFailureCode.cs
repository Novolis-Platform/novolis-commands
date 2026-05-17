namespace Novolis.Commands;

public enum ParseFailureCode
{
    EmptyPrompt,
    UnknownContext,
    UnknownCommand,
    MissingArgument,
    InvalidArgument,
    AmbiguousCommand,
    NotAllowed,
    RequiresConfirmation
}
