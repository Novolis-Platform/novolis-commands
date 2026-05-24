# Novolis.Commands.Abstractions

Command envelope, parse results, queue contracts, and processor interfaces shared by the Novolis command stack.

## Install

```bash
dotnet add package Novolis.Commands.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Commands;

var envelope = new CommandEnvelope
{
    Id = new CommandId("helm.fire"),
    Name = "fire",
    OriginalPrompt = "helm fire",
    ContextWord = "helm",
    Arguments = new Dictionary<string, object?>(),
};

ParseResult result = ParseResult.Succeeded(envelope);
if (result.Success)
    await queue.EnqueueAsync(result.Command!, cancellationToken);
```

Implement `ICommandProcessor<TContext>` in your app and consume `ICommandQueue` from `Novolis.Commands.Queueing`.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Commands.Engine` | Natural-language parsing and `CommandRegistry` |
| `Novolis.Commands.Queueing` | Channel-backed queue and runner |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/design.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages).
