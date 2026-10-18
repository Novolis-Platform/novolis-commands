namespace Novolis.Commands.Navigation;

/// <summary>Describes a state transition emitted by a navigation controller.</summary>
public sealed class MobileNavigationChangedEventArgs : EventArgs
{
    /// <summary>Creates a state transition event.</summary>
    public MobileNavigationChangedEventArgs(
        MobileNavigationState previous,
        MobileNavigationState current)
    {
        Previous = previous ?? throw new ArgumentNullException(nameof(previous));
        Current = current ?? throw new ArgumentNullException(nameof(current));
    }

    /// <summary>State before the transition.</summary>
    public MobileNavigationState Previous { get; }

    /// <summary>State after the transition.</summary>
    public MobileNavigationState Current { get; }
}
