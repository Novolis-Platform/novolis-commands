# Getting started

## Install

```bash
dotnet add package Novolis.Commands.Abstractions --version 1.0.0
dotnet add package Novolis.Commands.Engine --version 1.0.0
dotnet add package Novolis.Commands.Queueing --version 1.0.0
dotnet add package Novolis.Commands.DependencyInjection --version 1.0.0
```

## Register commands

```csharp
services.AddSingleton<ICommandContextResolver<MyContext>, MyContextResolver>();
services.AddNovolisCommands<MyContext>(
    registry =>
    {
        registry.Add("helm.set-speed", "helm", ["warp"],
            CommandArgumentDefinition.Integer("warp", required: true));
        registry.Add("helm.set-heading", "helm", ["heading", "set course"], "heading3d");
    },
    configureEngine: options =>
    {
        options.ArgumentParsers.Register("heading3d", new MyHeadingArgumentParser());
    });
services.AddSingleton<ICommandProcessor<MyContext>, MyProcessor>();
services.AddNovolisCommandRunner<MyContext>();
```

Implement `ICommandContextResolver<TContext>` with context aliases (e.g. `pilot` → `helm`). Orders must start with a station prefix or alias.

## Custom argument parser (3D heading example)

```csharp
public sealed class MyHeadingArgumentParser : ICommandArgumentParser
{
    public bool TryParse(
        CommandDefinition definition,
        IReadOnlyList<string> argumentTokens,
        out IReadOnlyDictionary<string, object?> arguments,
        out ParseFailure? failure)
    {
        // Parse MARK/BY, comma decimals, etc.
    }
}
```

## Parse and enqueue

```csharp
var result = await engine.ParseCommandAsync("helm heading 270", context);
if (result.Success)
    await queue.EnqueueAsync(result.Command!);
else if (result.Suggestions.Count > 0)
    // show result.Suggestions to the player
```

## Run the queue

```csharp
var runner = sp.GetRequiredService<CommandQueueRunner<MyContext>>();
await runner.RunAsync(context, cancellationToken);
```

See [design.md](design.md) for parse vs processor boundaries and [migration-v02-to-v10.md](migration-v02-to-v10.md) when upgrading from preview.
