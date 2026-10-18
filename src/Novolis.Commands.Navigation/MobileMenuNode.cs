namespace Novolis.Commands.Navigation;

/// <summary>Immutable semantic menu node consumed by native mobile renderers.</summary>
public sealed record MobileMenuNode
{
    /// <summary>Creates a menu node with a stable identifier and display label.</summary>
    public MobileMenuNode(
        string id,
        string label,
        MobileMenuNodeKind kind = MobileMenuNodeKind.Destination)
    {
        Id = id?.Trim() ?? string.Empty;
        Label = label?.Trim() ?? string.Empty;
        Kind = kind;
    }

    /// <summary>Stable identifier used by navigation and automation.</summary>
    public string Id { get; init; }

    /// <summary>Primary visible label.</summary>
    public string Label { get; init; }

    /// <summary>Optional supporting text shown below the label.</summary>
    public string? Subtitle { get; init; }

    /// <summary>Stable icon key interpreted by the platform renderer.</summary>
    public string? IconKey { get; init; }

    /// <summary>Optional short status badge.</summary>
    public string? Badge { get; init; }

    /// <summary>Semantic kind of this node.</summary>
    public MobileMenuNodeKind Kind { get; init; }

    /// <summary>Placement requested for a root node.</summary>
    public MobileMenuPlacement Placement { get; init; } = MobileMenuPlacement.Secondary;

    /// <summary>Route opened by a destination node.</summary>
    public string? RouteId { get; init; }

    /// <summary>Command raised by an action, toggle, or choice node.</summary>
    public string? CommandId { get; init; }

    /// <summary>Whether this node should be offered by a renderer.</summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>Whether this node can currently be activated.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Explanation shown when <see cref="IsEnabled"/> is false.</summary>
    public string? DisabledReason { get; init; }

    /// <summary>Whether a toggle node is currently on.</summary>
    public bool IsChecked { get; init; }

    /// <summary>Whether a destination is selected in the current shell state.</summary>
    public bool IsSelected { get; init; }

    /// <summary>Whether the current menu overlay closes after activation.</summary>
    public bool CloseAfterActivate { get; init; } = true;

    /// <summary>Child nodes opened by a group node.</summary>
    public IReadOnlyList<MobileMenuNode> Children { get; init; } = [];

    /// <summary>Creates a node with a copied child collection.</summary>
    public MobileMenuNode WithChildren(IEnumerable<MobileMenuNode>? children) =>
        this with { Children = children?.ToArray() ?? [] };
}
