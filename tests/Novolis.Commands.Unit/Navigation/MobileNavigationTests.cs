using Novolis.Commands.Navigation;

namespace Novolis.Commands.Navigation.Tests;

public sealed class MobileNavigationTests
{
    [Test]
    public async Task Validate_accepts_primary_destinations_and_secondary_groups()
    {
        var tree = CreateTree();

        await Assert.That(tree.Validate()).IsEmpty();
        await Assert.That(tree.Find("settings")?.Children).HasSingleItem();
    }

    [Test]
    public async Task Validate_reports_missing_route_and_duplicate_ids()
    {
        var tree = new MobileMenuTree(
        [
            new MobileMenuNode("home", "Home")
            {
                Placement = MobileMenuPlacement.Primary,
            },
            new MobileMenuNode("home", "Duplicate", MobileMenuNodeKind.Action),
        ]);

        var diagnostics = tree.Validate();

        await Assert.That(diagnostics.Select(item => item.Code))
            .Contains("NAV003");
        await Assert.That(diagnostics.Select(item => item.Code))
            .Contains("NAV007");
    }

    [Test]
    public async Task Controller_starts_on_the_first_enabled_primary_destination()
    {
        var controller = new MobileNavigationController(CreateTree());

        await Assert.That(controller.State.CurrentRouteId).IsEqualTo("today");
        await Assert.That(controller.State.SelectedPrimaryId).IsEqualTo("today");
        await Assert.That(controller.GetPrimaryNodes().Single(node => node.Id == "today").IsSelected)
            .IsTrue();
    }

    [Test]
    public async Task Select_primary_resets_the_route_stack()
    {
        var controller = new MobileNavigationController(CreateTree());
        controller.Activate("details");
        controller.SelectPrimary("places");

        await Assert.That(controller.State.RouteStack).IsEquivalentTo(["places"]);
        await Assert.That(controller.State.SelectedPrimaryId).IsEqualTo("places");
    }

    [Test]
    public async Task Group_activation_opens_children_and_back_returns_to_menu_root()
    {
        var controller = new MobileNavigationController(CreateTree());

        controller.OpenSecondaryMenu();
        controller.Activate("settings");

        await Assert.That(controller.State.Overlay)
            .IsEqualTo(MobileMenuOverlay.SecondaryMenu);
        await Assert.That(controller.State.CurrentMenuNodeId).IsEqualTo("settings");
        await Assert.That(controller.GetOpenMenuNodes().Single().Id).IsEqualTo("about");

        controller.Back();
        await Assert.That(controller.State.CurrentMenuNodeId).IsNull();
        controller.Back();
        await Assert.That(controller.State.Overlay).IsEqualTo(MobileMenuOverlay.None);
    }

    [Test]
    public async Task Action_activation_emits_command_and_closes_overlay()
    {
        var controller = new MobileNavigationController(CreateTree());
        MobileMenuActivationEventArgs? activation = null;
        controller.Activated += (_, args) => activation = args;

        controller.OpenSecondaryMenu();
        controller.Activate("settings");
        controller.Activate("about");

        await Assert.That(activation?.CommandId).IsEqualTo("about.open");
        await Assert.That(controller.State.Overlay).IsEqualTo(MobileMenuOverlay.None);
    }

    [Test]
    public async Task Back_pops_a_child_route_after_closing_overlays()
    {
        var controller = new MobileNavigationController(CreateTree());
        controller.Activate("details");
        controller.OpenSecondaryMenu();

        await Assert.That(controller.Back()).IsTrue();
        await Assert.That(controller.State.Overlay).IsEqualTo(MobileMenuOverlay.None);
        await Assert.That(controller.Back()).IsTrue();
        await Assert.That(controller.State.CurrentRouteId).IsEqualTo("today");
    }

    [Test]
    public async Task Disabled_nodes_are_not_activated()
    {
        var controller = new MobileNavigationController(
            new MobileMenuTree(
            [
                new MobileMenuNode("home", "Home")
                {
                    Placement = MobileMenuPlacement.Primary,
                    RouteId = "home",
                },
                new MobileMenuNode("locked", "Locked")
                {
                    RouteId = "locked",
                    IsEnabled = false,
                    DisabledReason = "Sign in first.",
                },
            ]));

        await Assert.That(controller.Activate("locked")).IsFalse();
        await Assert.That(controller.State.CurrentRouteId).IsEqualTo("home");
    }

    static MobileMenuTree CreateTree() =>
        new(
        [
            new MobileMenuNode("today", "Today")
            {
                IconKey = "today",
                Placement = MobileMenuPlacement.Primary,
                RouteId = "today",
            },
            new MobileMenuNode("places", "Places")
            {
                IconKey = "places",
                Placement = MobileMenuPlacement.Primary,
                RouteId = "places",
            },
            new MobileMenuNode("details", "Details")
            {
                RouteId = "details",
            },
            new MobileMenuNode("settings", "Settings", MobileMenuNodeKind.Group)
            {
                IconKey = "settings",
                Children =
                [
                    new MobileMenuNode("about", "About", MobileMenuNodeKind.Action)
                    {
                        CommandId = "about.open",
                    },
                ],
            },
        ]);
}
