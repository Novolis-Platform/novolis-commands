<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start — embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Commands.Abstractions` | `dotnet add package Novolis.Commands.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Abstractions/README.md) |
| `Novolis.Commands.DependencyInjection` | `dotnet add package Novolis.Commands.DependencyInjection` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.DependencyInjection/README.md) |
| `Novolis.Commands.Engine` | `dotnet add package Novolis.Commands.Engine` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Engine/README.md) |
| `Novolis.Commands.Queueing` | `dotnet add package Novolis.Commands.Queueing` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Queueing/README.md) |
| `Novolis.Commands.Testing` | `dotnet add package Novolis.Commands.Testing` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Testing/README.md) |

For NuGet.org and Visual Studio, the **embedded** README.md inside each package is authoritative.

<!-- novolis-package-index:end -->

# novolis-commands

Parse natural-language prompts into command envelopes, queue them, and run them with interrupt-aware cancellation.

## Packages

| Package | Description |
|---------|-------------|
| `Novolis.Commands.Abstractions` | Contracts: parse results, envelopes, queue and processor interfaces |
| `Novolis.Commands.Engine` | Text parsing and command registry (no domain execution) |
| `Novolis.Commands.Queueing` | Channel-backed queue and queue runner |
| `Novolis.Commands.DependencyInjection` | `AddNovolisCommands<TContext>()` registration |
| `Novolis.Commands.Testing` | Test doubles and helpers |

## Milestone v0

```text
string prompt → ParseResult → CommandEnvelope on ICommandQueue
```

See [docs/design.md](docs/design.md) for parse vs processor boundaries.

## Build

```bash
dotnet build
dotnet test
```

