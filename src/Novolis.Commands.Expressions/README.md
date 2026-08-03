<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-commands">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Commands.Expressions

Parse function-call style prompts into structured `FunctionCall` values:

- Single: `Line(0, 1, 2, 3)` or bare `Undo`
- Nested: `Line(Point(0.0,1.0), Point(1.0,1.0))`
- Scripts: `Line(...); Circle(...); Extrude(2.4);` (`;` between calls)

```csharp
using Novolis.Commands.Expressions;

var script = FunctionCallParser.TryParseScript(
    "Line(Point(0,1), Point(1,1)); Circle(Point(2,2), 0.5);");
foreach (var call in script.Calls)
{
    // call.Name, call.Arguments (may be nested Call)
}
```


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

