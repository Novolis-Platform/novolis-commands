using Novolis.Commands.Engine;

namespace Novolis.Commands.Engine.Tests.Support;

/// <summary>
/// Bridge Commander–style registry used for broad parse coverage.
/// </summary>
public static class CommandEngineTestRegistry
{
    /// <summary>Exact Bridge Commander station registry (no lab/alpha fixtures).</summary>
    public static ICommandRegistry CreateBridge() =>
        new CommandRegistryBuilder()
            .Add("helm.set-heading", "helm",
                ["set heading to", "set heading", "heading"])
            .Add("helm.come-about", "helm", ["come about"])
            .Add("helm.all-ahead-full", "helm", ["all ahead full", "ahead full"])
            .Add("helm.full-stop", "helm", ["full stop", "all stop"])
            .Add("helm.set-speed", "helm", ["set warp", "warp"],
                CommandArgumentDefinition.Integer("warp", required: true))
            .Add("tactical.lock-target", "tactical",
                ["lock target", "target lock", "target the closest enemy", "target closest enemy"])
            .Add("tactical.fire-weapons", "tactical", ["fire weapons", "fire"])
            .Add("engineering.divert-shields", "engineering", ["divert shields", "shields max"])
            .Add("engineering.divert-weapons", "engineering", ["divert weapons"])
            .Add("engineering.repair", "engineering", ["repair", "damage control"])
            .Add("nav.set-course", "nav", ["set course", "course"],
                CommandArgumentDefinition.String("destination", required: true))
            .Add("comms.hail", "comms", ["hail", "open channel"])
            .Add("crew.dismiss-personnel", "admin", ["fire"])
            .Build();

    public static ICommandRegistry Create() =>
        new CommandRegistryBuilder()
            .Add("helm.set-heading", "helm", ["set heading", "heading"],
                CommandArgumentDefinition.Integer("heading", required: true))
            .Add("helm.full-stop", "helm", ["full stop", "all stop"])
            .Add("helm.set-speed", "helm", ["set warp", "warp"],
                CommandArgumentDefinition.Integer("warp", required: true))
            .Add("tactical.lock-target", "tactical", ["lock target", "target lock"])
            .Add("tactical.fire-weapons", "tactical", ["fire weapons", "fire"])
            .Add("engineering.divert-shields", "engineering", ["divert shields", "shields max"])
            .Add("engineering.divert-weapons", "engineering", ["divert weapons"])
            .Add("engineering.repair", "engineering", ["repair", "damage control"])
            .Add("nav.set-course", "nav", ["set course", "course"],
                CommandArgumentDefinition.String("destination", required: true))
            .Add("comms.hail", "comms", ["hail", "open channel"])
            .Add("crew.dismiss-personnel", "admin", ["fire"])
            .Add("alpha.scan", "alpha", ["scan"])
            .Add("beta.scan", "beta", ["scan"])
            .Build();
}
