namespace Novolis.Agent.Session;

/// <summary>In-process player session: snapshot, commands, and push events at decision points.</summary>
public interface IGameSession
{
    SessionHelloResponseDto Hello();

    SessionSnapshotDto Snapshot();

    SessionActionsResponseDto Actions();

    SessionCommandResultDto Execute(SessionCommandDto command);

    SessionCommandResultDto Continue();

    void Subscribe();

    event Action<SessionDecisionEventDto>? Decision;

    event Action<SessionChangedEventDto>? Changed;

    event Action<SessionActionResultEventDto>? ActionResult;
}

/// <summary>Optional transport host surface (LocalIpc, HTTP, TCP JSONL, stdio).</summary>
public interface ISessionTransport
{
    string Kind { get; }

    ValueTask StartAsync(CancellationToken cancellationToken = default);

    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
