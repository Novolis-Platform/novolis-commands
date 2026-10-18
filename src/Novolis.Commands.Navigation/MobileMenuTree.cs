namespace Novolis.Commands.Navigation;

/// <summary>Immutable root of a host-neutral mobile menu tree.</summary>
public sealed class MobileMenuTree
{
    /// <summary>Maximum number of persistent destinations in mobile bottom navigation.</summary>
    public const int MaximumPrimaryNodes = 5;

    readonly IReadOnlyList<MobileMenuNode> _rootNodes;

    /// <summary>Creates a tree from its root nodes.</summary>
    public MobileMenuTree(IEnumerable<MobileMenuNode>? rootNodes)
    {
        _rootNodes = rootNodes?.ToArray() ?? [];
    }

    /// <summary>Root nodes in their declared presentation order.</summary>
    public IReadOnlyList<MobileMenuNode> RootNodes => _rootNodes;

    /// <summary>Finds a node anywhere in the tree by stable identifier.</summary>
    public MobileMenuNode? Find(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        foreach (var node in _rootNodes)
        {
            if (Find(node, nodeId) is { } found)
                return found;
        }

        return null;
    }

    /// <summary>Reports structural errors without coupling callers to a UI framework.</summary>
    public IReadOnlyList<MobileMenuTreeDiagnostic> Validate()
    {
        var diagnostics = new List<MobileMenuTreeDiagnostic>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var primaryCount = 0;

        foreach (var node in _rootNodes)
            ValidateNode(node, isRoot: true, ids, diagnostics, ref primaryCount);

        if (primaryCount > MaximumPrimaryNodes)
        {
            diagnostics.Add(
                new MobileMenuTreeDiagnostic(
                    "NAV001",
                    $"A mobile menu can expose at most {MaximumPrimaryNodes} primary destinations."));
        }

        return diagnostics;
    }

    static MobileMenuNode? Find(MobileMenuNode node, string nodeId)
    {
        if (string.Equals(node.Id, nodeId, StringComparison.Ordinal))
            return node;

        foreach (var child in node.Children)
        {
            if (Find(child, nodeId) is { } found)
                return found;
        }

        return null;
    }

    static void ValidateNode(
        MobileMenuNode node,
        bool isRoot,
        HashSet<string> ids,
        List<MobileMenuTreeDiagnostic> diagnostics,
        ref int primaryCount)
    {
        if (string.IsNullOrWhiteSpace(node.Id))
        {
            diagnostics.Add(
                new MobileMenuTreeDiagnostic(
                    "NAV002",
                    "Every menu node must have a stable identifier."));
        }
        else if (!ids.Add(node.Id))
        {
            diagnostics.Add(
                new MobileMenuTreeDiagnostic(
                    "NAV003",
                    $"Menu node id '{node.Id}' is used more than once.",
                    node.Id));
        }

        if (node.Kind is not MobileMenuNodeKind.Separator
            && string.IsNullOrWhiteSpace(node.Label))
        {
            diagnostics.Add(
                new MobileMenuTreeDiagnostic(
                    "NAV004",
                    "Every non-separator menu node must have a label.",
                    node.Id));
        }

        if (isRoot && node.Placement == MobileMenuPlacement.Primary)
        {
            primaryCount++;
            if (node.Kind != MobileMenuNodeKind.Destination)
            {
                diagnostics.Add(
                    new MobileMenuTreeDiagnostic(
                        "NAV005",
                        "Only destination nodes may occupy primary mobile navigation.",
                        node.Id));
            }
        }
        else if (!isRoot && node.Placement == MobileMenuPlacement.Primary)
        {
            diagnostics.Add(
                new MobileMenuTreeDiagnostic(
                    "NAV006",
                    "Primary placement is only valid for root destinations.",
                    node.Id));
        }

        switch (node.Kind)
        {
            case MobileMenuNodeKind.Destination when string.IsNullOrWhiteSpace(node.RouteId):
                diagnostics.Add(
                    new MobileMenuTreeDiagnostic(
                        "NAV007",
                        "Destination nodes must provide a route id.",
                        node.Id));
                break;
            case MobileMenuNodeKind.Group when node.Children.Count == 0:
                diagnostics.Add(
                    new MobileMenuTreeDiagnostic(
                        "NAV008",
                        "Group nodes must contain at least one child.",
                        node.Id));
                break;
            case MobileMenuNodeKind.Group
                when !string.IsNullOrWhiteSpace(node.RouteId)
                     || !string.IsNullOrWhiteSpace(node.CommandId):
                diagnostics.Add(
                    new MobileMenuTreeDiagnostic(
                        "NAV009",
                        "Group nodes cannot provide a route or command id.",
                        node.Id));
                break;
            case MobileMenuNodeKind.Action
                or MobileMenuNodeKind.Toggle
                or MobileMenuNodeKind.Choice
                when string.IsNullOrWhiteSpace(node.CommandId):
                diagnostics.Add(
                    new MobileMenuTreeDiagnostic(
                        "NAV010",
                        "Action, toggle, and choice nodes must provide a command id.",
                        node.Id));
                break;
            case MobileMenuNodeKind.Action
                or MobileMenuNodeKind.Toggle
                or MobileMenuNodeKind.Choice
                when node.Children.Count > 0:
                diagnostics.Add(
                    new MobileMenuTreeDiagnostic(
                        "NAV011",
                        "Action, toggle, and choice nodes cannot contain children.",
                        node.Id));
                break;
            case MobileMenuNodeKind.Separator
                when node.Children.Count > 0
                     || !string.IsNullOrWhiteSpace(node.RouteId)
                     || !string.IsNullOrWhiteSpace(node.CommandId):
                diagnostics.Add(
                    new MobileMenuTreeDiagnostic(
                        "NAV012",
                        "Separator nodes cannot contain children, routes, or commands.",
                        node.Id));
                break;
        }

        foreach (var child in node.Children)
            ValidateNode(child, isRoot: false, ids, diagnostics, ref primaryCount);
    }
}
