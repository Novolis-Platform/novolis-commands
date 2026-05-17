# Getting started

## Install

```bash
dotnet add package Novolis.Commands.Abstractions --version 0.1.0-preview.1
dotnet add package Novolis.Commands.Engine --version 0.1.0-preview.1
dotnet add package Novolis.Commands.Queueing --version 0.1.0-preview.1
dotnet add package Novolis.Commands.DependencyInjection --version 0.1.0-preview.1
```

## Register commands

```csharp
services.AddSingleton<ICommandContextResolver<MyContext>, MyContextResolver>();
services.AddNovolisCommands<MyContext>(registry =>
{
    registry.Add("helm.set-heading", "helm", ["heading"],
        CommandArgumentDefinition.Integer("heading"));
});
services.AddSingleton<ICommandProcessor<MyContext>, MyProcessor>();
services.AddSingleton<ICommandQueue, ChannelCommandQueue>();
```

## Parse and enqueue

```csharp
var result = await engine.ParseCommandAsync("helm heading 270", context);
if (result.Success)
    await queue.EnqueueAsync(result.Command!);
```

## Run the queue

```csharp
var runner = new CommandQueueRunner<MyContext>(queue, processor);
await runner.RunAsync(context, cancellationToken);
```

See [design.md](design.md) for parse vs processor boundaries.
