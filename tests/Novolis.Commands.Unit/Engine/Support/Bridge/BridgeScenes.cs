namespace Novolis.Commands.Engine.Tests.Support.Bridge;

/// <summary>
/// Scripted Star Trek bridge scenes for USS Novolis — parse and simulation coverage.
/// </summary>
public static class BridgeScenes
{
    public static IEnumerable<BridgeScene> All() =>
    [
        Kr12RedAlertEncounter(),
        AlphaCentauriFirstContact(),
        EvasiveManeuversAndBelay(),
        DamageControlAfterExchange(),
        CaptainCallsForHelp(),
        ImpatientCaptainRejectedOrders(),
        PersonnelTransfer(),
        QuietNightWatch(),
        NaturalOrdersFromLog()
    ];

    public static IEnumerable<BridgeSceneOrder> AllOrders() =>
        All().SelectMany(scene => scene.Orders.Select(order => order with
        {
            Beat = $"{scene.Title} — {order.Beat}"
        }));

    /// <summary>
    /// Classic engagement: hostile KR-12, shields, lock, evasive, warp, fire, repair.
    /// </summary>
    public static BridgeScene Kr12RedAlertEncounter() => new(
        Title: "The KR-12 Incident",
        Epigraph:
            "Stardate 47829.1. A hostile frigate drops from warp off the port bow. " +
            "Red alert echoes through the USS Novolis.",
        Orders:
        [
            Order(
                "La Forge hears the klaxon first.",
                "La Forge",
                "engineering divert shields",
                "engineering.divert-shields",
                "engineering"),
            Order(
                "Worf tracks the contact on tactical.",
                "Worf",
                "tactical lock target",
                "tactical.lock-target",
                "tactical"),
            Order(
                "Riker leans toward conn.",
                "Riker",
                "helm heading 270",
                "helm.set-heading",
                "helm",
                new Dictionary<string, object?> { ["heading"] = 270 }),
            Order(
                "Picard's voice is steady.",
                "Picard",
                "conn warp 8",
                "helm.set-speed",
                "helm",
                new Dictionary<string, object?> { ["warp"] = 8 }),
            Order(
                "Worf's jaw tightens.",
                "Worf",
                "weaps fire",
                "tactical.fire-weapons",
                "tactical"),
            Order(
                "Dr. Crusher's tone crackles from sickbay.",
                "Crusher",
                "eng repair",
                "engineering.repair",
                "engineering")
        ],
        EndState: new BridgeSceneExpectation(
            Heading: 270,
            Warp: 8,
            ShieldsMin: 95,
            HullMin: 100,
            TargetLocked: true,
            TargetName: "Hostile frigate KR-12",
            StatusContains: "hull repair"));

    /// <summary>
    /// Peaceful approach: lay in course, hail, all stop.
    /// </summary>
    public static BridgeScene AlphaCentauriFirstContact() => new(
        Title: "First Contact at Alpha Centauri",
        Epigraph:
            "The Novolis glides toward the triple star. Picard wants diplomacy before drama.",
        Orders:
        [
            Order(
                "Data plots the approach.",
                "Data",
                "nav set course alpha centauri",
                "nav.set-course",
                "nav",
                new Dictionary<string, object?> { ["destination"] = "alpha centauri" }),
            Order(
                "Uhura opens the channel.",
                "Uhura",
                "comms hail",
                "comms.hail",
                "comms"),
            Order(
                "Picard raises one hand.",
                "Picard",
                "helm full stop",
                "helm.full-stop",
                "helm")
        ],
        EndState: new BridgeSceneExpectation(
            Warp: 0,
            StatusContains: "all stop"));

    /// <summary>
    /// Long repair interrupted by belay — tests emergency built-in.
    /// </summary>
    public static BridgeScene EvasiveManeuversAndBelay() => new(
        Title: "Belay That",
        Epigraph:
            "Phaser fire rocks the bridge. The captain changes his mind mid-order.",
        Orders:
        [
            Order("Shields first.", "Riker", "eng shields max", "engineering.divert-shields", "engineering"),
            Order("Evasive.", "Riker", "pilot heading 090", "helm.set-heading", "helm",
                new Dictionary<string, object?> { ["heading"] = 90 }),
            Order(
                "Picard slaps the armrest.",
                "Picard",
                "belay that",
                BuiltInCommands.BelayThat,
                expectedPriority: CommandPriority.Emergency,
                interrupts: true)
        ],
        EndState: new BridgeSceneExpectation(Heading: 90, ShieldsMin: 95));

    /// <summary>
    /// Post-battle damage control and weapons power.
    /// </summary>
    public static BridgeScene DamageControlAfterExchange() => new(
        Title: "Damage Control",
        Epigraph: "Smoke wisps from a ceiling panel. La Forge doesn't sit down.",
        Orders:
        [
            Order("Structural integrity.", "La Forge", "eng damage control", "engineering.repair", "engineering"),
            Order("Phasers need juice.", "La Forge", "engineering divert weapons", "engineering.divert-weapons", "engineering"),
            Order("One more pass.", "Worf", "guns lock target", "tactical.lock-target", "tactical"),
            Order("Fire at will.", "Picard", "tac fire weapons", "tactical.fire-weapons", "tactical")
        ],
        EndState: new BridgeSceneExpectation(
            TargetLocked: true,
            HullMin: 100,
            StatusContains: "weapons fired"));

    /// <summary>
    /// Captain uses the computer, not raw station prefixes.
    /// </summary>
    public static BridgeScene CaptainCallsForHelp() => new(
        Title: "Computer, Assist",
        Epigraph: "Picard stares at the blank viewer. The crew waits.",
        Orders:
        [
            Order("General reference.", "Picard", "help", BuiltInCommands.Help),
            Order(
                "Tactical station only.",
                "Picard",
                "help tactical",
                BuiltInCommands.Help,
                arguments: new Dictionary<string, object?> { ["topic"] = "tactical" }),
            Order("Repeat the last order.", "Picard", "repeat last", BuiltInCommands.RepeatLast)
        ]);

    /// <summary>
    /// Dramatic lines that must not parse — no station prefix.
    /// </summary>
    public static BridgeScene ImpatientCaptainRejectedOrders() => new(
        Title: "Make It So?",
        Epigraph: "Holodeck habits die hard. The computer expects discipline.",
        Orders:
        [
            Reject("Picard forgets the prefix.", "Picard", "make it so", ParseFailureCode.UnknownContext),
            Reject("Worf barks without context.", "Worf", "fire phasers", ParseFailureCode.UnknownContext),
            Reject("Riker trails off.", "Riker", "helm", ParseFailureCode.UnknownCommand),
            Reject("Empty channel.", null, "   ", ParseFailureCode.EmptyPrompt)
        ]);

    /// <summary>
    /// Admin station — personnel transfer euphemism.
    /// </summary>
    public static BridgeScene PersonnelTransfer() => new(
        Title: "The Transfer",
        Epigraph: "HR will not be pleased. The captain signs the PADD anyway.",
        Orders:
        [
            Order("Logged.", "Picard", "admin fire", "crew.dismiss-personnel", "admin")
        ],
        EndState: new BridgeSceneExpectation(StatusContains: "personnel transfer"));

    /// <summary>
    /// Night watch — minimal orders, quiet ship.
    /// </summary>
    public static BridgeScene QuietNightWatch() => new(
        Title: "Night Watch",
        Epigraph: "0300 hours. The stars drift. Ensign Kim has the conn.",
        Orders:
        [
            Order("Course correction.", "Kim", "helm set heading 180", "helm.set-heading", "helm",
                new Dictionary<string, object?> { ["heading"] = 180 }),
            Order("Resume cruise.", "Kim", "helm warp 5", "helm.set-speed", "helm",
                new Dictionary<string, object?> { ["warp"] = 5 }),
            Order("Clear the backlog.", "Kim", "clear queue", BuiltInCommands.ClearQueue,
                expectedPriority: CommandPriority.High)
        ],
        EndState: new BridgeSceneExpectation(Heading: 180, Warp: 5, StatusContains: "queue cleared"));

    /// <summary>
    /// Exact lines from the captain's log that previously failed parsing.
    /// </summary>
    public static BridgeScene NaturalOrdersFromLog() => new(
        Title: "Natural Orders",
        Epigraph: "The captain speaks the way humans do. The computer learns to listen.",
        Orders:
        [
            Order("Reverse course.", "Picard", "helm come about", "helm.come-about", "helm"),
            Order("Maximum warp.", "Riker", "helm all ahead full", "helm.all-ahead-full", "helm"),
            Order("Worf acquires the hostile.", "Worf", "weaps target the closest enemy", "tactical.lock-target", "tactical"),
            Order(
                "Picard with punctuation and 3D heading.",
                "Picard",
                "helm, set heading to 122 by 180",
                "helm.set-heading",
                "helm",
                new Dictionary<string, object?> { ["heading"] = 122.0, ["headingBy"] = 180.0 })
        ],
        EndState: new BridgeSceneExpectation(
            Heading: 122,
            HeadingBy: 180,
            Warp: 9,
            TargetLocked: true));

    private static BridgeSceneOrder Order(
        string beat,
        string? speaker,
        string spoken,
        string expectedCommand,
        string? context = null,
        IReadOnlyDictionary<string, object?>? arguments = null,
        CommandPriority? expectedPriority = null,
        bool? interrupts = null) =>
        new(
            Beat: beat,
            Spoken: spoken,
            ExpectedCommand: expectedCommand,
            Arguments: arguments,
            ExpectedContext: context,
            Speaker: speaker,
            ExpectedPriority: expectedPriority,
            Interrupts: interrupts);

    private static BridgeSceneOrder Reject(
        string beat,
        string? speaker,
        string spoken,
        ParseFailureCode failure) =>
        new(Beat: beat, Spoken: spoken, Speaker: speaker, ShouldFail: true, ExpectedFailure: failure);
}
