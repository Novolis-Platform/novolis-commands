# Novolis.Commands.DependencyInjection

Registers the Novolis command registry, channel queue, matcher, and `ICommandEngine<TContext>` in `Microsoft.Extensions.DependencyInjection`.

## Install

```bash
dotnet add package Novolis.Commands.DependencyInjection
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Microsoft.Extensions.DependencyInjection;
using Novolis.Commands.DependencyInjection;
using Novolis.Commands.Engine;

services.AddNovolisCommands<MyContext>(b => b
    .Add("fire", "helm", ["fire"]));

services.AddSingleton<ICommandContextResolver<MyContext>, MyResolver>();
services.AddNovolisCommandRunner<MyContext>();
```

You must register `ICommandContextResolver<TContext>` and `ICommandProcessor<TContext>` in the host.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Commands.Engine` | Parser and registry types |
| `Novolis.Commands.Queueing` | `ChannelCommandQueue` and runner |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/design.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages).
