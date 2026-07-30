namespace Novolis.Agent.Session;

public static class SessionMethodNames
{
    public const string Hello = "session.hello";
    public const string Snapshot = "session.snapshot";
    public const string Actions = "session.actions";
    public const string Command = "session.command";
    public const string Continue = "session.continue";
    public const string Subscribe = "session.subscribe";

    public const string Decision = "session.decision";
    public const string Changed = "session.changed";
    public const string ActionResult = "session.actionResult";
}

public static class SessionRpcMessageKinds
{
    public const string Request = "request";
    public const string Response = "response";
    public const string Event = "event";
    public const string Fault = "fault";
}

public static class SessionActionIds
{
    public const string Travel = "travel";
    public const string AcceptSpot = "acceptSpot";
    public const string AcceptCharter = "acceptCharter";
    public const string MarketBuy = "marketBuy";
    public const string MarketSell = "marketSell";
    public const string Depart = "depart";
    public const string RefuseStandby = "refuseStandby";
    public const string AcceptStandby = "acceptStandby";
    public const string Wait = "wait";
    public const string Premium = "premium";
    public const string Overhaul = "overhaul";
    public const string Step = "step";
    public const string Continue = "continue";
    public const string Resume = "resume";
    public const string Save = "save";
    public const string SetClock = "setClock";
    public const string CancelStack = "cancelStack";
    public const string PrepareDepart = "prepareDepart";
}
