# Novolis.Commands.Navigation

Headless menu-tree and mobile navigation contracts for Novolis hosts.

## Install

```powershell
dotnet add package Novolis.Commands.Navigation
```

## Quick start

```csharp
var tree = new MobileMenuTree(
[
    new MobileMenuNode("today", "Today")
    {
        IconKey = "today",
        Placement = MobileMenuPlacement.Primary,
        RouteId = "today",
    },
    new MobileMenuNode("settings", "Settings", MobileMenuNodeKind.Group)
    {
        IconKey = "settings",
        Children =
        [
            new MobileMenuNode("about", "About")
            {
                CommandId = "about.open",
                Kind = MobileMenuNodeKind.Action,
            },
        ],
    },
]);

var navigation = new MobileNavigationController(tree);
navigation.Activated += (_, activation) =>
    Console.WriteLine(activation.CommandId ?? activation.RouteId);
```

The package contains no MAUI, Avalonia, or game-host dependency. Platform
renderers own the native app bar, bottom navigation, overflow/drawer, sheet,
dialog, accessibility, and Back handling. Applications own route content and
command execution.

Primary destinations are capped at five so they remain usable in mobile
bottom navigation. Secondary groups can be drilled into as a menu tree.

## Related packages

- `Novolis.Maui.Navigation`
- `Novolis.Avalonia.Navigation`

## More documentation

See the repository design notes in `d:\novolis\novolis-commands\README.md`.
