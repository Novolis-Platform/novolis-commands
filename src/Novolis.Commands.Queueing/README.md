<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-commands">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Commands.Queueing

Channel-backed `ICommandQueue` and `CommandQueueRunner<TContext>` for background command processing.

## Install

```bash
dotnet add package Novolis.Commands.Queueing
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Commands;
using Novolis.Commands.Queueing;

ICommandQueue queue = new ChannelCommandQueue();
var runner = new CommandQueueRunner<MyContext>(queue, processor);

await queue.EnqueueAsync(envelope, cancellationToken);
await runner.RunAsync(context, cancellationToken);
```

Pair with `AddNovolisCommands` / `AddNovolisCommandRunner` from `Novolis.Commands.DependencyInjection`.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Commands.Abstractions` | `CommandEnvelope`, `ICommandProcessor<T>` |
| `Novolis.Commands.DependencyInjection` | One-line host registration |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/design.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages).

