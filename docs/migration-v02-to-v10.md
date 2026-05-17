# Migration: 0.2 preview → 1.0

## Argument parsers

Register domain parsers in the host and reference them from the registry:

```csharp
var options = new CommandEngineOptions();
options.ArgumentParsers.Register("heading3d", new MyHeadingArgumentParser());

services.AddNovolisCommands<MyContext>(
    registry => registry.Add("helm.set-heading", "helm", ["heading"], "heading3d"),
    configureEngine: o => o.ArgumentParsers.Register("heading3d", new MyHeadingArgumentParser()));
```

Remove any reliance on the engine’s built-in `helm.set-heading` grammar.

## Context resolver

Remove `GetActiveContextWord` from your `ICommandContextResolver<TContext>` implementation. Orders must include an explicit station prefix (or context alias).

## Parse failures

Replace usages of `ParseFailureCode.NotAllowed` and `RequiresConfirmation` if referenced.

## Clear queue

`CommandQueueRunner` now calls `ICommandQueue.ClearPendingAsync()` when an envelope has `CancelsQueuedCommands`. Custom queue implementations must implement drain semantics.
