namespace Novolis.Commands.Navigation;

/// <summary>
/// Applies deterministic mobile navigation behavior to a semantic menu tree.
/// Native renderers observe state and activation events but own their controls.
/// </summary>
public sealed class MobileNavigationController
{
    MobileNavigationState _state;

    /// <summary>Creates a controller over a validated menu tree.</summary>
    public MobileNavigationController(MobileMenuTree tree)
    {
        Tree = ValidateTree(tree);
        _state = CreateInitialState(Tree);
    }

    /// <summary>Raised after the route, selection, or overlay changes.</summary>
    public event EventHandler<MobileNavigationChangedEventArgs>? Changed;

    /// <summary>Raised when a destination or command node is activated.</summary>
    public event EventHandler<MobileMenuActivationEventArgs>? Activated;

    /// <summary>Current menu tree.</summary>
    public MobileMenuTree Tree { get; private set; }

    /// <summary>Current immutable navigation state.</summary>
    public MobileNavigationState State => _state;

    /// <summary>Returns visible primary destinations with current selection applied.</summary>
    public IReadOnlyList<MobileMenuNode> GetPrimaryNodes() =>
        Tree.RootNodes
            .Where(node =>
                node.IsVisible
                && node.Placement == MobileMenuPlacement.Primary
                && node.Kind == MobileMenuNodeKind.Destination)
            .Select(node => node with { IsSelected = node.Id == _state.SelectedPrimaryId })
            .ToArray();

    /// <summary>Returns visible root nodes that are not primary destinations.</summary>
    public IReadOnlyList<MobileMenuNode> GetSecondaryNodes() =>
        Tree.RootNodes
            .Where(node =>
                node.IsVisible
                && node.Placement != MobileMenuPlacement.Primary)
            .ToArray();

    /// <summary>Returns the rows for the currently open menu branch.</summary>
    public IReadOnlyList<MobileMenuNode> GetOpenMenuNodes()
    {
        if (_state.Overlay == MobileMenuOverlay.None)
            return [];

        if (_state.CurrentMenuNodeId is not { } parentId)
            return GetSecondaryNodes();

        return Tree.Find(parentId)?.Children
            .Where(node => node.IsVisible)
            .Select(node => node with { IsSelected = false })
            .ToArray()
            ?? [];
    }

    /// <summary>Replaces the tree while retaining a still-valid current route.</summary>
    public void ReplaceTree(MobileMenuTree tree)
    {
        var nextTree = ValidateTree(tree);
        var currentRoute = _state.CurrentRouteId;
        var selected = nextTree.RootNodes.FirstOrDefault(node =>
            node.IsVisible
            && node.IsEnabled
            && node.Placement == MobileMenuPlacement.Primary
            && node.Kind == MobileMenuNodeKind.Destination
            && string.Equals(node.RouteId, currentRoute, StringComparison.Ordinal));
        selected ??= nextTree.RootNodes.FirstOrDefault(node =>
            node.IsVisible
            && node.IsEnabled
            && node.Placement == MobileMenuPlacement.Primary
            && node.Kind == MobileMenuNodeKind.Destination);

        Tree = nextTree;
        var routeStack = selected?.RouteId is { } route
            ? new[] { route }
            : [];
        SetState(
            new MobileNavigationState(
                routeStack,
                selected?.Id,
                MobileMenuOverlay.None,
                []));
    }

    /// <summary>Opens the secondary root menu.</summary>
    public bool OpenSecondaryMenu()
    {
        if (!GetSecondaryNodes().Any(node => node.Kind != MobileMenuNodeKind.Separator))
            return false;

        SetState(
            new MobileNavigationState(
                _state.RouteStack,
                _state.SelectedPrimaryId,
                MobileMenuOverlay.SecondaryMenu,
                []));
        return true;
    }

    /// <summary>Closes the active menu overlay.</summary>
    public bool CloseOverlay()
    {
        if (_state.Overlay == MobileMenuOverlay.None)
            return false;

        SetState(
            new MobileNavigationState(
                _state.RouteStack,
                _state.SelectedPrimaryId,
                MobileMenuOverlay.None,
                []));
        return true;
    }

    /// <summary>Activates a visible node in the current navigation context.</summary>
    public bool Activate(string nodeId)
    {
        var node = GetOpenNode(nodeId);
        if (node is null
            || !node.IsEnabled
            || node.Kind == MobileMenuNodeKind.Separator)
        {
            return false;
        }

        switch (node.Kind)
        {
            case MobileMenuNodeKind.Group:
                OpenGroup(node);
                return true;
            case MobileMenuNodeKind.Destination:
                Navigate(node);
                Activated?.Invoke(this, new MobileMenuActivationEventArgs(node));
                return true;
            case MobileMenuNodeKind.Action:
            case MobileMenuNodeKind.Toggle:
            case MobileMenuNodeKind.Choice:
                if (node.CloseAfterActivate)
                    CloseOverlay();
                Activated?.Invoke(this, new MobileMenuActivationEventArgs(node));
                return true;
            default:
                return false;
        }
    }

    /// <summary>Selects a primary destination and resets its route stack.</summary>
    public bool SelectPrimary(string nodeId) =>
        GetPrimaryNodes().Any(node => node.Id == nodeId)
        && Activate(nodeId);

    /// <summary>Applies native Back behavior to overlays and then the route stack.</summary>
    public bool Back()
    {
        if (_state.Overlay != MobileMenuOverlay.None)
        {
            if (_state.MenuPath.Count > 0)
            {
                SetState(
                    new MobileNavigationState(
                        _state.RouteStack,
                        _state.SelectedPrimaryId,
                        _state.Overlay,
                        _state.MenuPath.Count == 1
                            ? []
                            : _state.MenuPath.Take(_state.MenuPath.Count - 1)));
            }
            else
            {
                CloseOverlay();
            }

            return true;
        }

        if (_state.RouteStack.Count <= 1)
            return false;

        SetState(
            new MobileNavigationState(
                _state.RouteStack.Take(_state.RouteStack.Count - 1),
                _state.SelectedPrimaryId,
                MobileMenuOverlay.None,
                []));
        return true;
    }

    MobileMenuNode? GetOpenNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        if (_state.Overlay == MobileMenuOverlay.None)
        {
            return Tree.RootNodes.FirstOrDefault(node =>
                node.IsVisible
                && string.Equals(node.Id, nodeId, StringComparison.Ordinal));
        }

        return GetOpenMenuNodes().FirstOrDefault(node =>
            string.Equals(node.Id, nodeId, StringComparison.Ordinal));
    }

    void OpenGroup(MobileMenuNode node)
    {
        var nextPath = _state.Overlay == MobileMenuOverlay.None
            ? new[] { node.Id }
            : _state.MenuPath.Append(node.Id).ToArray();
        SetState(
            new MobileNavigationState(
                _state.RouteStack,
                _state.SelectedPrimaryId,
                MobileMenuOverlay.SecondaryMenu,
                nextPath));
    }

    void Navigate(MobileMenuNode node)
    {
        if (string.IsNullOrWhiteSpace(node.RouteId))
            return;

        var isPrimary = node.Placement == MobileMenuPlacement.Primary
            && Tree.RootNodes.Any(root => root.Id == node.Id);
        var routes = isPrimary
            ? new[] { node.RouteId }
            : _state.RouteStack.Append(node.RouteId).ToArray();
        var selected = isPrimary ? node.Id : _state.SelectedPrimaryId;
        SetState(
            new MobileNavigationState(
                routes,
                selected,
                MobileMenuOverlay.None,
                []));
    }

    void SetState(MobileNavigationState next)
    {
        if (AreEqual(_state, next))
            return;

        var previous = _state;
        _state = next;
        Changed?.Invoke(this, new MobileNavigationChangedEventArgs(previous, next));
    }

    static MobileMenuTree ValidateTree(MobileMenuTree tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        var diagnostics = tree.Validate();
        if (diagnostics.Count > 0)
        {
            var summary = string.Join(
                " ",
                diagnostics.Select(diagnostic =>
                    diagnostic.NodeId is null
                        ? $"{diagnostic.Code}: {diagnostic.Message}"
                        : $"{diagnostic.Code} ({diagnostic.NodeId}): {diagnostic.Message}"));
            throw new ArgumentException(
                $"The mobile menu tree is invalid. {summary}",
                nameof(tree));
        }

        return tree;
    }

    static MobileNavigationState CreateInitialState(MobileMenuTree tree)
    {
        var first = tree.RootNodes.FirstOrDefault(node =>
            node.IsVisible
            && node.IsEnabled
            && node.Placement == MobileMenuPlacement.Primary
            && node.Kind == MobileMenuNodeKind.Destination
            && !string.IsNullOrWhiteSpace(node.RouteId));
        return first is null
            ? new MobileNavigationState()
            : new MobileNavigationState([first.RouteId!], first.Id);
    }

    static bool AreEqual(MobileNavigationState left, MobileNavigationState right) =>
        left.Overlay == right.Overlay
        && string.Equals(
            left.SelectedPrimaryId,
            right.SelectedPrimaryId,
            StringComparison.Ordinal)
        && left.RouteStack.SequenceEqual(right.RouteStack, StringComparer.Ordinal)
        && left.MenuPath.SequenceEqual(right.MenuPath, StringComparer.Ordinal);
}
