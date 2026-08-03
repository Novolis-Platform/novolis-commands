<!-- novolis-marketing:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-brand-transparent.svg" width="360" alt="Novolis"/>
  </a>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/banners/novolis-commands.svg" width="100%" alt="novolis-commands"/>
</p>

<p align="center">
  <strong>Interrupt-aware command queues</strong><br/>
  Parse prompts into command envelopes, queue them, run with cancellation.
</p>

<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-commands/actions"><img src="https://img.shields.io/github/actions/workflow/status/Novolis-Platform/novolis-commands/merge.yml?branch=main&label=merge&logo=github" alt="merge"/></a>
  <a href="https://github.com/orgs/Novolis-Platform/packages?repo_name=novolis-commands"><img src="https://img.shields.io/badge/packages-GitHub%20Packages-0a7ea3?logo=nuget" alt="packages"/></a>
  <a href="https://github.com/Novolis-Platform"><img src="https://img.shields.io/badge/org-Novolis--Platform-111827" alt="org"/></a>
</p>

<p align="center">
  <a href="https://nuget.pkg.github.com/Novolis-Platform/index.json"><code>https://nuget.pkg.github.com/Novolis-Platform/index.json</code></a>
  ·
  <a href="https://github.com/Novolis-Platform/.github/blob/main/profile/README.md">Org landing</a>
  ·
  <a href="https://github.com/Novolis-Platform/novolis-governance">Governance</a>
</p>

---
<!-- novolis-marketing:end -->
<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start — embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Commands.Abstractions` | `dotnet add package Novolis.Commands.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Abstractions/README.md) |
| `Novolis.Commands.DependencyInjection` | `dotnet add package Novolis.Commands.DependencyInjection` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.DependencyInjection/README.md) |
| `Novolis.Commands.Engine` | `dotnet add package Novolis.Commands.Engine` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Engine/README.md) |
| `Novolis.Commands.Expressions` | `dotnet add package Novolis.Commands.Expressions` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Expressions/README.md) |
| `Novolis.Commands.Queueing` | `dotnet add package Novolis.Commands.Queueing` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Queueing/README.md) |
| `Novolis.Commands.Testing` | `dotnet add package Novolis.Commands.Testing` | [README](https://github.com/Novolis-Platform/novolis-commands/blob/main/src/Novolis.Commands.Testing/README.md) |

For NuGet.org and Visual Studio, the **embedded** README.md inside each package is authoritative.

<!-- novolis-package-index:end -->
# novolis-commands

Parse natural-language prompts into command envelopes, queue them, and run them with interrupt-aware cancellation. Agent Surface packages live in **novolis-agent**.

## Packages

| Package | Description |
|---------|-------------|
| `Novolis.Commands.Abstractions` | Contracts: parse results, envelopes, queue and processor interfaces |
| `Novolis.Commands.Engine` | Text parsing and command registry (no domain execution) |
| `Novolis.Commands.Queueing` | Channel-backed queue and queue runner |
| `Novolis.Commands.DependencyInjection` | `AddNovolisCommands<TContext>()` registration |
| `Novolis.Commands.Testing` | Test doubles and helpers |
| `Novolis.Commands.Expressions` | Function-call prompt parser (`Line(0,1)`) |

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

