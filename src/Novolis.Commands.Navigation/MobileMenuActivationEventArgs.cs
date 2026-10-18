namespace Novolis.Commands.Navigation;

/// <summary>Describes a route or command activation requested by a menu node.</summary>
public sealed class MobileMenuActivationEventArgs : EventArgs
{
    /// <summary>Creates an activation event for a menu node.</summary>
    public MobileMenuActivationEventArgs(MobileMenuNode node)
    {
        Node = node ?? throw new ArgumentNullException(nameof(node));
    }

    /// <summary>Node that was activated.</summary>
    public MobileMenuNode Node { get; }

    /// <summary>Route requested by a destination node, if present.</summary>
    public string? RouteId => Node.RouteId;

    /// <summary>Command requested by an action node, if present.</summary>
    public string? CommandId => Node.CommandId;
}
