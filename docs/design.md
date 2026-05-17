# Design

## Boundary

Novolis.Commands stops at **intent**, not **execution**.

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
| `clear queue` | `system.clear-queue` | High | no | yes (runner drains pending via `ClearPendingAsync`) |
| `repeat last` | `system.repeat-last` | Normal | no | no |
| `help` / `help <topic>` | `system.help` | Normal | no | no |

`repeat last` execution is host responsibility (re-enqueue or replay last envelope).

## Registry

Domain commands are registered by the host via `CommandRegistryBuilder` or DI `configureRegistry`. `CommandRegistryValidator` runs on `Build()` and at engine construction (parser keys).

Optional `CommandDefinition.ArgumentParserKey` selects a host-registered `ICommandArgumentParser` for argument tokens after verb phrase matching.

## Ambiguity and suggestions

When multiple definitions share a verb, the parser returns `ParseFailureCode.AmbiguousCommand` and ranked `CommandCandidate` entries. Confidence is a heuristic (0–1), not NLP.

On `UnknownCommand`, `ParseResult.Suggestions` lists up to three close registered verb phrases (Levenshtein distance) for the current context.

## Packages

- **Abstractions** — contracts only, no Channels or Microsoft.Extensions
- **Engine** — parse pipeline, registry validation, argument parser registry
- **Queueing** — `Channel<CommandEnvelope>` hidden behind `ICommandQueue`
- **DependencyInjection** — `AddNovolisCommands<TContext>()`, `AddNovolisCommandRunner<TContext>()`
- **Testing** — fakes for tests and consumers
