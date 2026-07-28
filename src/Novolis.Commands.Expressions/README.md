# Novolis.Commands.Expressions

Parse function-call style prompts (`Line(0, 1)`, `Undo`) into structured `FunctionCall` values.

## Install

```bash
dotnet add package Novolis.Commands.Expressions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Commands.Expressions;

var result = FunctionCallParser.TryParse("Line(0, 1, 2, 3)");
if (result.Success)
{
    var call = result.Call!;
    // call.Name == "Line"
    // call.Arguments[0].Number == 0
}
```

Bare verbs without parentheses are supported (`Undo`, `Delete`) and report `HasParentheses == false`.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Commands.Engine` | Natural-language verb-phrase parsing |
| `Novolis.Commands.Abstractions` | Envelopes and queue contracts |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
