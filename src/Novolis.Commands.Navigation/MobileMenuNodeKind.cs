namespace Novolis.Commands.Navigation;

/// <summary>Semantic role of a node in a mobile menu tree.</summary>
public enum MobileMenuNodeKind
{
    /// <summary>Opens a route in the navigation stack.</summary>
    Destination,

    /// <summary>Opens child menu nodes.</summary>
    Group,

    /// <summary>Raises a command without changing the route.</summary>
    Action,

    /// <summary>Raises a command representing a boolean choice.</summary>
    Toggle,

    /// <summary>Raises a command representing a value choice.</summary>
    Choice,

    /// <summary>Visual separation between menu rows.</summary>
    Separator,
}
