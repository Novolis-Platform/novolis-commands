# Game session protocol

Player decision-point protocol: snapshot, typed commands, and push events over multiple transports. Domain rules stay in the product app; this package is wire + session contracts only.

Single package: **`Novolis.Agent.Session`** (`IGameSession`, MessagePack DTOs, `SessionClient`, `SessionHost`, `SessionStdioHost`).

## Wire methods (v1)

| Method | Kind | Purpose |
|--------|------|---------|
| `session.hello` | req/res | Version, app id, capabilities (`protocolVersion = "1.0"`) |
| `session.snapshot` | req/res | Full player-facing state |
| `session.actions` | req/res | Allowed actions + disabled reasons |
| `session.command` | req/res | Execute one typed action |
| `session.continue` | req/res | Release day gate |
| `session.subscribe` | req/res | Client wants push events |
| `session.decision` | event | Pause at decision point |
| `session.changed` | event | Snapshot-relevant change (coalesced) |
| `session.actionResult` | event | Async order settlement |

## Transport matrix

| Transport | v1 |
|-----------|----|
| LocalIpc | `SessionHost` / `SessionClient` |
| In-process | UI / CLI bind `IGameSession` |
| MCP | Adapter tools (`session_*`) |
| Stdio JSONL | `SessionStdioHost` |
| Tcp/Http | `ISessionTransport` interface only |

## LocalIpc

Enable host by binding `SessionHost` (Sins does this via `SessionSurface.AttachAll` so all hooks are always live, no env vars needed).
Optional endpoint: pipe `novolis-game-session-sins`.

```csharp
var host = SessionHost.Attach(session, SessionEndpoints.DefaultPipeName);
```

## NuGet

Consume via GitHub Packages `2026.1.*`. Local workspace: `-p:NovolisUseProjectReferences=true` (no folder feeds).
