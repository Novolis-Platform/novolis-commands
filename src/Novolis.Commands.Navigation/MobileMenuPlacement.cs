namespace Novolis.Commands.Navigation;

/// <summary>Preferred placement of a root menu node in a mobile shell.</summary>
public enum MobileMenuPlacement
{
    /// <summary>Persistent primary destination, normally shown in bottom navigation.</summary>
    Primary,

    /// <summary>Secondary destination or action, normally shown in overflow navigation.</summary>
    Secondary,

    /// <summary>Contextual action for the current route.</summary>
    Context,
}
