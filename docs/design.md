# Design

## Boundary

Novolis.Commands v0 stops at **intent**, not **execution**.

| Layer | Responsibility |
|-------|----------------|
| `ICommandEngine<TContext>` | Parse `string prompt` → `ParseResult` with optional `CommandEnvelope` |
| `ICommandQueue` | Enqueue envelopes for asynchronous processing |
| `ICommandProcessor<TContext>` | Host-defined domain behavior |
| `CommandQueueRunner<TContext>` | Dequeue, honor interrupt flags, link cancellation per command. Does not block reading the next envelope while a command is in flight; on interrupt, cancels and awaits the in-flight task before starting the next |

The parser never calls `ICommandProcessor` and never mutates simulation or game state.

## Built-in commands

Phrase → envelope only (handled in `BuiltInCommandMatcher`):

| Phrase | Name | Priority | Interrupts | Cancels queue |
|--------|------|----------|------------|---------------|
| `belay that` | `system.belay-that` | Emergency | yes | no |
| `clear queue` | `system.clear-queue` | High | no | yes |
| `repeat last` | `system.repeat-last` | Normal | no | no |

Queue draining for `CancelsQueuedCommands` is a host/processor policy in v0; the runner does not drain the channel.

## Registry

Domain commands (e.g. `helm.set-heading`) are registered by the host via `CommandRegistryBuilder` or DI `configureRegistry`. The engine ships no game-specific definitions.

## Ambiguity

When multiple definitions share a verb, the parser returns `ParseFailureCode.AmbiguousCommand` and ranked `CommandCandidate` entries. Confidence is a v0 heuristic (active context boost, not NLP).

## Packages

- **Abstractions** — contracts only, no Channels or Microsoft.Extensions
- **Engine** — parse pipeline
- **Queueing** — `Channel<CommandEnvelope>` hidden behind `ICommandQueue`
- **DependencyInjection** — `AddNovolisCommands<TContext>()`
- **Testing** — fakes for tests and consumers
