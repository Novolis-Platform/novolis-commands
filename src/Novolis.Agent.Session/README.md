# Novolis.Agent.Session

Live **control session** protocol for agent desks and mid-run takeovers: snapshot, typed commands, push events.

Formerly `Novolis.Game.Session` in `novolis-gaming` — control wire belongs with agent/commands tooling, not game authoring.

## Install

```bash
dotnet add package Novolis.Agent.Session
```

## Transports (same `IGameSession`)

| Transport | Notes |
|-----------|--------|
| **HTTP REST + SSE** | Default via `SessionSurface.AttachAll` |
| **LocalIpc** MessagePack | Named pipe |
| **TCP JSONL** | Optional second hook |
| **Stdio JSONL** | `SessionStdioHost` |
| **In-process** | Bind `IGameSession` directly |

```csharp
using Novolis.Agent.Session;

var surface = SessionSurface.AttachAll(session, preferredPipeName: SessionEndpoints.DefaultPipeName);
```

### HTTP routes

| Method | Path |
|--------|------|
| GET | `/health`, `/session/hello`, `/session/snapshot`, `/session/actions` |
| POST | `/session/command`, `/session/continue`, `/session/subscribe`, `/session/rpc` |
| GET SSE | `/session/events` |

## Wire methods (v1)

`session.hello` · `snapshot` · `actions` · `command` · `continue` · `subscribe`  
events: `decision` · `changed` · `actionResult`

## Docs

- [Session protocol](https://github.com/Novolis-Platform/novolis-commands/blob/main/docs/session-protocol.md)
