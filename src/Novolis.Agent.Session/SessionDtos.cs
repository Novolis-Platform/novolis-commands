using MessagePack;

namespace Novolis.Agent.Session;

[MessagePackObject]
public sealed class SessionHelloRequestDto
{
    [Key(0)] public long Sequence { get; set; }
}

[MessagePackObject]
public sealed class SessionHelloResponseDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public string ProtocolVersion { get; set; } = "1.1";
    [Key(2)] public string AppId { get; set; } = "";
    [Key(3)] public string AppTitle { get; set; } = "";
    [Key(4)] public int ProcessId { get; set; }
    [Key(5)] public string[] Capabilities { get; set; } = [];
}

[MessagePackObject]
public sealed class SessionSnapshotRequestDto
{
    [Key(0)] public long Sequence { get; set; }
}

[MessagePackObject]
public sealed class SessionActionsRequestDto
{
    [Key(0)] public long Sequence { get; set; }
}

[MessagePackObject]
public sealed class SessionSubscribeRequestDto
{
    [Key(0)] public long Sequence { get; set; }
}

[MessagePackObject]
public sealed class SessionSubscribeResponseDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public bool Ok { get; set; } = true;
}

[MessagePackObject]
public sealed class SessionContinueRequestDto
{
    [Key(0)] public long Sequence { get; set; }
}

[MessagePackObject]
public sealed class SessionCommandRequestDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public SessionCommandDto Command { get; set; } = new();
}

/// <summary>Generic command envelope: action id plus string parameter bag.</summary>
[MessagePackObject]
public sealed class SessionCommandDto
{
    [Key(0)] public string ActionId { get; set; } = "";

    /// <summary>
    /// Opaque string parameters (e.g. destSystemId, index, sku, qty, attention, speed, prepare, label).
    /// </summary>
    [Key(1)] public Dictionary<string, string> Params { get; set; } = new(StringComparer.Ordinal);

    public string? Get(string key) =>
        Params.TryGetValue(key, out var value) ? value : null;

    public bool TryGetInt(string key, out int value)
    {
        value = 0;
        var raw = Get(key);
        return raw is not null && int.TryParse(raw, out value);
    }

    public bool TryGetDouble(string key, out double value)
    {
        value = 0;
        var raw = Get(key);
        return raw is not null && double.TryParse(raw, out value);
    }

    public bool TryGetBool(string key, out bool value)
    {
        value = false;
        var raw = Get(key);
        if (raw is null)
            return false;
        if (bool.TryParse(raw, out value))
            return true;
        if (raw is "1" or "yes")
        {
            value = true;
            return true;
        }

        if (raw is "0" or "no")
        {
            value = false;
            return true;
        }

        return false;
    }

    public SessionCommandDto With(string key, string? value)
    {
        if (value is not null)
            Params[key] = value;
        return this;
    }

    public SessionCommandDto With(string key, int value) => With(key, value.ToString());

    public SessionCommandDto With(string key, double value) => With(key, value.ToString("R"));

    public SessionCommandDto With(string key, bool value) => With(key, value ? "true" : "false");
}

/// <summary>Well-known command parameter keys (apps may add others).</summary>
public static class SessionCommandKeys
{
    public const string DestSystemId = "destSystemId";
    public const string OriginSystemId = "originSystemId";
    public const string Index = "index";
    public const string Sku = "sku";
    public const string Qty = "qty";
    public const string Profile = "profile";
    public const string Label = "label";
    public const string Attention = "attention";
    public const string Speed = "speed";
    public const string Prepare = "prepare";
}

[MessagePackObject]
public sealed class SessionCommandResultDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public bool Ok { get; set; }
    [Key(2)] public string ActionId { get; set; } = "";
    [Key(3)] public string Message { get; set; } = "";
    [Key(4)] public string? ErrorCode { get; set; }
    [Key(5)] public SessionSnapshotDto? Snapshot { get; set; }
}

[MessagePackObject]
public sealed class SessionActionDto
{
    [Key(0)] public string Id { get; set; } = "";
    [Key(1)] public string Label { get; set; } = "";
    [Key(2)] public bool Enabled { get; set; }
    [Key(3)] public string? DisabledReason { get; set; }
}

[MessagePackObject]
public sealed class SessionActionsResponseDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public SessionActionDto[] Actions { get; set; } = [];
}

[MessagePackObject]
public sealed class SessionLastActionDto
{
    [Key(0)] public string ActionId { get; set; } = "";
    [Key(1)] public bool Ok { get; set; }
    [Key(2)] public string Message { get; set; } = "";
    [Key(3)] public string? ErrorCode { get; set; }
}

[MessagePackObject]
public sealed class SessionBoardItemDto
{
    [Key(0)] public int Index { get; set; }
    [Key(1)] public string Id { get; set; } = "";
    [Key(2)] public string Label { get; set; } = "";
    [Key(3)] public string Detail { get; set; } = "";
    [Key(4)] public bool CanAct { get; set; }
}

[MessagePackObject]
public sealed class SessionBoardDto
{
    [Key(0)] public string Id { get; set; } = "";
    [Key(1)] public SessionBoardItemDto[] Items { get; set; } = [];
}

/// <summary>Well-known board ids (apps may add others).</summary>
public static class SessionBoardIds
{
    public const string SpotFreight = "spotFreight";
    public const string GoodsCharters = "goodsCharters";
    public const string MarketLots = "marketLots";
}

/// <summary>Well-known status line keys (apps may add others).</summary>
public static class SessionLineKeys
{
    public const string Voyage = "voyage";
    public const string Hull = "hull";
    public const string Cash = "cash";
    public const string Standing = "standing";
    public const string Decision = "decision";
    public const string Coach = "coach";
    public const string SoftFail = "softFail";
    public const string Survival = "survival";
    public const string Mesh = "mesh";
    public const string Hold = "hold";
    public const string Pace = "pace";
}

[MessagePackObject]
public sealed class SessionSnapshotDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public int Day { get; set; }
    [Key(2)] public string SeedHash { get; set; } = "";
    [Key(3)] public string HubId { get; set; } = "";
    [Key(4)] public string HubName { get; set; } = "";
    [Key(5)] public string PauseReason { get; set; } = "Running";

    /// <summary>Opaque display lines keyed by <see cref="SessionLineKeys"/> (or app-specific).</summary>
    [Key(6)] public Dictionary<string, string> StatusLines { get; set; } = new(StringComparer.Ordinal);

    [Key(7)] public bool Underway { get; set; }
    [Key(8)] public bool DockedIdle { get; set; }
    [Key(9)] public bool Complete { get; set; }
    [Key(10)] public bool SoftFail { get; set; }
    [Key(11)] public bool StandbyOffer { get; set; }
    [Key(12)] public string? TravelTargetSystemId { get; set; }
    [Key(13)] public string[] RouteSystemIds { get; set; } = [];
    [Key(14)] public SessionBoardDto[] Boards { get; set; } = [];
    [Key(15)] public string[] Manifest { get; set; } = [];
    [Key(16)] public SessionActionDto[] Actions { get; set; } = [];
    [Key(17)] public SessionLastActionDto? LastAction { get; set; }
    [Key(18)] public string Attention { get; set; } = "runAlways";
    [Key(19)] public double SimSpeedScale { get; set; } = 1.0;
    [Key(20)] public string[] IntentStack { get; set; } = [];
    [Key(21)] public double MapX { get; set; }
    [Key(22)] public double MapY { get; set; }
    [Key(23)] public bool MapVisible { get; set; }
    [Key(24)] public double GameHoursPerRealMinute { get; set; }
    [Key(25)] public double SessionGameHoursPerRealMinute { get; set; }

    public string Line(string key) =>
        StatusLines.TryGetValue(key, out var value) ? value : "";

    public SessionBoardItemDto[] BoardItems(string boardId)
    {
        foreach (var board in Boards)
        {
            if (string.Equals(board.Id, boardId, StringComparison.Ordinal))
                return board.Items;
        }

        return [];
    }
}

[MessagePackObject]
public sealed class SessionDecisionEventDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public int Day { get; set; }
    [Key(2)] public string HubId { get; set; } = "";
    [Key(3)] public string DecisionLine { get; set; } = "";
    [Key(4)] public SessionSnapshotDto? Snapshot { get; set; }
}

[MessagePackObject]
public sealed class SessionChangedEventDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public string Reason { get; set; } = "";
    [Key(2)] public SessionSnapshotDto? Snapshot { get; set; }
}

[MessagePackObject]
public sealed class SessionActionResultEventDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public string ActionId { get; set; } = "";
    [Key(2)] public bool Ok { get; set; }
    [Key(3)] public string Message { get; set; } = "";
    [Key(4)] public string? ErrorCode { get; set; }
    [Key(5)] public SessionSnapshotDto? Snapshot { get; set; }
}

[MessagePackObject]
public sealed class SessionFaultDto
{
    [Key(0)] public long Sequence { get; set; }
    [Key(1)] public string Message { get; set; } = "";
}
