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
