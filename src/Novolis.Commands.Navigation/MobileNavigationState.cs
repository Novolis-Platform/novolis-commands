namespace Novolis.Commands.Navigation;

/// <summary>Immutable navigation state shared by native mobile renderers.</summary>
public sealed class MobileNavigationState
{
    /// <summary>Creates a navigation state snapshot.</summary>
    public MobileNavigationState(
        IEnumerable<string>? routeStack = null,
        string? selectedPrimaryId = null,
        MobileMenuOverlay overlay = MobileMenuOverlay.None,
        IEnumerable<string>? menuPath = null)
    {
        RouteStack = routeStack?.ToArray() ?? [];
        SelectedPrimaryId = selectedPrimaryId;
        Overlay = overlay;
        MenuPath = menuPath?.ToArray() ?? [];
    }

    /// <summary>Route identifiers from the root route to the current route.</summary>
    public IReadOnlyList<string> RouteStack { get; }

    /// <summary>Selected root destination, if one is active.</summary>
    public string? SelectedPrimaryId { get; }

    /// <summary>Currently open native menu surface.</summary>
    public MobileMenuOverlay Overlay { get; }

    /// <summary>Group identifiers from the secondary menu root to its current branch.</summary>
    public IReadOnlyList<string> MenuPath { get; }

    /// <summary>Current route, or null before a destination has been opened.</summary>
    public string? CurrentRouteId => RouteStack.LastOrDefault();

    /// <summary>Current menu group, or null when the secondary menu is at its root.</summary>
    public string? CurrentMenuNodeId => MenuPath.LastOrDefault();
}
