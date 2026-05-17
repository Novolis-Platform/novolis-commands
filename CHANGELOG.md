# Changelog

## 1.0.0

### Breaking

- Removed `ICommandContextResolver<TContext>.GetActiveContextWord`.
- Removed `ParseFailureCode.NotAllowed` and `ParseFailureCode.RequiresConfirmation`.
- Removed built-in `HeadingArgumentParser` and `helm.set-heading` hardcoding from the engine. Hosts register `ICommandArgumentParser` via `CommandEngineOptions.ArgumentParsers` and `CommandDefinition.ArgumentParserKey`.
- Removed `CommandEngineOptions.UseLegacyHeadingParser`.

### Added

- `ICommandArgumentParser` and `CommandArgumentParserRegistry`.
- `CommandRegistryValidator` (duplicate names/phrases, empty verbs, unknown parser keys).
- `CommandEngineOptions` and extended `AddNovolisCommands` / `AddNovolisCommandRunner` DI helpers.
- `ParseResult.Suggestions` (Levenshtein on registered verb phrases for unknown commands).
- `ICommandQueue.ClearPendingAsync`; `CommandQueueRunner` drains pending items when `CancelsQueuedCommands` is set.
- Built-in `help` / `help <topic>` documented in design.

## 0.2.0-preview.1

### Added

- Pluggable argument parsers (additive; legacy heading path deprecated until 1.0).
- Registry validation at build time.
- Queue semantics documentation and tests.

## 0.1.0-preview.1

Initial preview.
