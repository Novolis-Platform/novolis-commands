namespace Novolis.Commands.Navigation;

/// <summary>Logical overlay state for a mobile menu renderer.</summary>
public enum MobileMenuOverlay
{
    /// <summary>No menu overlay is open.</summary>
    None,

    /// <summary>A secondary menu is open.</summary>
    SecondaryMenu,

    /// <summary>A transient action sheet is open.</summary>
    Sheet,

    /// <summary>A confirmation or choice dialog is open.</summary>
    Dialog,
}
