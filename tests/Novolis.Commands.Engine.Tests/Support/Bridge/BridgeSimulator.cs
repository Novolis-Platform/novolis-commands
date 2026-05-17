using Novolis.Commands;

namespace Novolis.Commands.Engine.Tests.Support.Bridge;

/// <summary>
/// Synchronous stand-in for Bridge Commander domain handling (no simulated delays).
/// </summary>
public sealed class BridgeSimulator
{
    public string ShipName { get; } = "USS Novolis";
    public int Heading { get; private set; } = 180;
    public int SpeedWarp { get; private set; } = 5;
    public int ShieldPercent { get; private set; } = 80;
    public int HullPercent { get; private set; } = 100;
    public bool TargetLocked { get; private set; }
    public string? TargetName { get; private set; }
    public string StatusLine { get; private set; } = "Standing by.";
    public string? LastExecutedCommand { get; private set; }
    public CommandEnvelope? LastEnvelope { get; private set; }
    public IReadOnlyList<string> Log { get; private set; } = [];

    private readonly List<string> _log = [];

    public ValueTask ProcessAsync(CommandEnvelope command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _log.Add($"▶ {command.OriginalPrompt}");

        switch (command.Name)
        {
            case BuiltInCommands.BelayThat:
                StatusLine = "Belay that! Current action cancelled.";
                _log.Add("  ↳ Emergency belay acknowledged.");
                return ValueTask.CompletedTask;

            case BuiltInCommands.ClearQueue:
                StatusLine = "Command queue cleared.";
                _log.Add("  ↳ Queued orders dismissed.");
                return ValueTask.CompletedTask;

            case BuiltInCommands.Help:
                StatusLine = "Computer: help displayed on main viewer.";
                _log.Add("  ↳ Help topic loaded.");
                return ValueTask.CompletedTask;

            case BuiltInCommands.RepeatLast:
                if (LastEnvelope is null)
                {
                    StatusLine = "Nothing to repeat.";
                    return ValueTask.CompletedTask;
                }

                return ProcessAsync(LastEnvelope, cancellationToken);

            case "helm.set-heading":
                Heading = (int)command.Arguments["heading"]! % 360;
                if (Heading < 0)
                    Heading += 360;
                StatusLine = $"Helm: course set to {Heading}°.";
                break;

            case "helm.full-stop":
                SpeedWarp = 0;
                StatusLine = "Helm: all stop.";
                break;

            case "helm.set-speed":
                SpeedWarp = Math.Clamp((int)command.Arguments["warp"]!, 0, 9);
                StatusLine = $"Helm: warp {SpeedWarp}.";
                break;

            case "tactical.lock-target":
                TargetLocked = true;
                TargetName = "Hostile frigate KR-12";
                StatusLine = "Tactical: target locked.";
                break;

            case "tactical.fire-weapons":
                StatusLine = TargetLocked
                    ? $"Tactical: weapons fired at {TargetName}."
                    : "Tactical: no target lock.";
                break;

            case "engineering.divert-shields":
                ShieldPercent = Math.Min(100, ShieldPercent + 15);
                StatusLine = "Engineering: power diverted to shields.";
                break;

            case "engineering.divert-weapons":
                ShieldPercent = Math.Max(0, ShieldPercent - 10);
                StatusLine = "Engineering: power diverted to weapons.";
                break;

            case "engineering.repair":
                HullPercent = Math.Min(100, HullPercent + 8);
                StatusLine = "Engineering: hull repair underway.";
                break;

            case "nav.set-course":
                StatusLine = $"Nav: course laid in for {command.Arguments["destination"]}.";
                break;

            case "comms.hail":
                StatusLine = "Comms: hail transmitted on open channel.";
                break;

            case "crew.dismiss-personnel":
                StatusLine = "Admin: personnel transfer logged.";
                break;

            default:
                StatusLine = $"Unknown command handler: {command.Name}";
                break;
        }

        _log.Add($"  ↳ {StatusLine}");
        LastEnvelope = command;
        LastExecutedCommand = command.Name;
        Log = _log.ToArray();
        return ValueTask.CompletedTask;
    }
}
