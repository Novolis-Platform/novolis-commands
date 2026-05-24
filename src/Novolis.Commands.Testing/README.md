# Novolis.Commands.Testing

Test doubles for command queues, context resolution, and registry-driven engines.

## Install

```bash
dotnet add package Novolis.Commands.Testing
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Commands.Testing;
using Novolis.Commands.Engine;

var queue = new RecordingCommandQueue();
var context = new TestCommandContext
{
    ContextAliases = new Dictionary<string, string> { ["h"] = "helm" },
};
var resolver = new TestCommandContextResolver();
var engine = new CommandEngine<TestCommandContext>(registry, resolver);

await queue.EnqueueAsync(envelope);
IReadOnlyList<CommandEnvelope> recorded = queue.Enqueued;
```

Use `ManualCommandQueue` when tests need explicit dequeue control.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Commands.Engine` | Production parser under test |
| `Novolis.Commands.Abstractions` | Envelope types |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/design.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages).
