<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-commands/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-commands/) · [Source](https://github.com/Novolis-Platform/novolis-commands)
<!-- novolis-pkg-brand:end -->

# Novolis.Commands.Engine

Natural-language command tokenizer, registry builder, and `CommandEngine<TContext>` parser.

## Install

```bash
dotnet add package Novolis.Commands.Engine
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Commands;
using Novolis.Commands.Engine;

var registry = new CommandRegistryBuilder()
    .Add("fire", "helm", ["fire"], CommandArgumentDefinition.None())
    .Build();

var engine = new CommandEngine<MyContext>(registry, resolver);
ParseResult parsed = await engine.ParseCommandAsync("helm fire", context);

string[] tokens = CommandTokenizer.Tokenize("helm fire");
```

Register commands in DI with `Novolis.Commands.DependencyInjection` for hosted apps.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Commands.Abstractions` | Envelopes and queue contracts |
| `Novolis.Commands.DependencyInjection` | `AddNovolisCommands<TContext>()` |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/design.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages).

