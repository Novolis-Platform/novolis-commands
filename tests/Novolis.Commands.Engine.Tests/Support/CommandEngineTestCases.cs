namespace Novolis.Commands.Engine.Tests.Support;

public static class CommandEngineTestCases
{
    public static IEnumerable<ParseCase> SuccessCases()
    {
        foreach (var c in BuiltInSuccessCases())
            yield return c;

        foreach (var c in HelmHeadingSweep())
            yield return c;

        foreach (var c in HelmWarpSweep())
            yield return c;

        foreach (var c in HelmPhraseSuccessCases())
            yield return c;

        foreach (var c in TacticalSuccessCases())
            yield return c;

        foreach (var c in EngineeringSuccessCases())
            yield return c;

        foreach (var c in NavSuccessCases())
            yield return c;

        foreach (var c in CommsSuccessCases())
            yield return c;

        foreach (var c in AdminSuccessCases())
            yield return c;

        foreach (var c in AliasSuccessCases())
            yield return c;

        foreach (var c in CaseAndWhitespaceSuccessCases())
            yield return c;
    }

    public static IEnumerable<ParseCase> FailureCases()
    {
        foreach (var c in EmptyAndContextFailureCases())
            yield return c;

        foreach (var c in UnknownCommandMatrix())
            yield return c;

        foreach (var c in ArgumentFailureCases())
            yield return c;

        foreach (var c in InvalidHeadingSweep())
            yield return c;
    }

    public static IEnumerable<ParseCase> AmbiguousCases()
    {
        yield return Case(
            "ambiguous-shared-scan",
            "shared scan",
            false,
            expectedFailure: ParseFailureCode.AmbiguousCommand,
            minimumCandidateCount: 2);

        yield return Case(
            "ambiguous-shared-lock",
            "shared lock target",
            false,
            expectedFailure: ParseFailureCode.AmbiguousCommand,
            minimumCandidateCount: 2);
    }

    private static IEnumerable<ParseCase> BuiltInSuccessCases()
    {
        yield return Case("builtin-belay", "belay that", true, BuiltInCommands.BelayThat);
        yield return Case("builtin-belay-upper", "BELAY THAT", true, BuiltInCommands.BelayThat);
        yield return Case("builtin-clear", "clear queue", true, BuiltInCommands.ClearQueue);
        yield return Case("builtin-repeat", "repeat last", true, BuiltInCommands.RepeatLast);
        yield return Case("builtin-help", "help", true, BuiltInCommands.Help);

        foreach (var topic in new[] { "tactical", "helm", "engineering", "nav", "system", "comms", "weapons" })
        {
            yield return Case(
                $"builtin-help-{topic}",
                $"help {topic}",
                true,
                BuiltInCommands.Help,
                arguments: new Dictionary<string, object?> { ["topic"] = topic });
        }
    }

    private static IEnumerable<ParseCase> HelmHeadingSweep()
    {
        for (var heading = 0; heading <= 359; heading++)
        {
            yield return Case(
                $"helm-h-{heading}",
                $"helm heading {heading}",
                true,
                "helm.set-heading",
                "helm",
                new Dictionary<string, object?> { ["heading"] = heading });

            yield return Case(
                $"helm-set-h-{heading}",
                $"helm set heading {heading}",
                true,
                "helm.set-heading",
                "helm",
                new Dictionary<string, object?> { ["heading"] = heading });
        }
    }

    private static IEnumerable<ParseCase> HelmWarpSweep()
    {
        for (var warp = 0; warp <= 15; warp++)
        {
            yield return Case(
                $"helm-warp-{warp}",
                $"helm warp {warp}",
                true,
                "helm.set-speed",
                "helm",
                new Dictionary<string, object?> { ["warp"] = warp });

            yield return Case(
                $"helm-set-warp-{warp}",
                $"helm set warp {warp}",
                true,
                "helm.set-speed",
                "helm",
                new Dictionary<string, object?> { ["warp"] = warp });
        }
    }

    private static IEnumerable<ParseCase> HelmPhraseSuccessCases()
    {
        yield return Case("helm-full-stop", "helm full stop", true, "helm.full-stop", "helm");
        yield return Case("helm-all-stop", "helm all stop", true, "helm.full-stop", "helm");
    }

    private static IEnumerable<ParseCase> TacticalSuccessCases()
    {
        foreach (var prefix in new[] { "tactical", "weaps", "guns", "tac" })
        {
            yield return Case($"{prefix}-lock", $"{prefix} lock target", true, "tactical.lock-target", "tactical");
            yield return Case($"{prefix}-fire", $"{prefix} fire weapons", true, "tactical.fire-weapons", "tactical");
            yield return Case($"{prefix}-fire-short", $"{prefix} fire", true, "tactical.fire-weapons", "tactical");
        }
    }

    private static IEnumerable<ParseCase> EngineeringSuccessCases()
    {
        foreach (var prefix in new[] { "engineering", "eng", "damage" })
        {
            yield return Case($"{prefix}-shields", $"{prefix} divert shields", true, "engineering.divert-shields", "engineering");
            yield return Case($"{prefix}-weapons", $"{prefix} divert weapons", true, "engineering.divert-weapons", "engineering");
            yield return Case($"{prefix}-repair", $"{prefix} repair", true, "engineering.repair", "engineering");
        }
    }

    private static IEnumerable<ParseCase> NavSuccessCases()
    {
        var destinations = new[]
        {
            "mars", "earth", "luna", "alpha centauri", "wolf 359", "sector 7",
            "deep space nine", "romulus", "vulcan", "bajor", "cardassia"
        };

        foreach (var dest in destinations)
        {
            var slug = dest.Replace(' ', '-');
            yield return Case(
                $"nav-course-{slug}",
                $"nav course {dest}",
                true,
                "nav.set-course",
                "nav",
                new Dictionary<string, object?> { ["destination"] = dest });

            yield return Case(
                $"nav-set-{slug}",
                $"nav set course {dest}",
                true,
                "nav.set-course",
                "nav",
                new Dictionary<string, object?> { ["destination"] = dest });
        }
    }

    private static IEnumerable<ParseCase> CommsSuccessCases()
    {
        yield return Case("comms-hail", "comms hail", true, "comms.hail", "comms");
        yield return Case("comms-open", "comms open channel", true, "comms.hail", "comms");
    }

    private static IEnumerable<ParseCase> AdminSuccessCases()
    {
        yield return Case("admin-fire", "admin fire", true, "crew.dismiss-personnel", "admin");
    }

    private static IEnumerable<ParseCase> AliasSuccessCases()
    {
        yield return Case("alias-pilot-heading", "pilot heading 90", true, "helm.set-heading", "helm",
            new Dictionary<string, object?> { ["heading"] = 90 });
        yield return Case("alias-conn-warp", "conn warp 3", true, "helm.set-speed", "helm",
            new Dictionary<string, object?> { ["warp"] = 3 });
    }

    private static IEnumerable<ParseCase> CaseAndWhitespaceSuccessCases()
    {
        yield return Case("case-helm", "HELM HEADING 180", true, "helm.set-heading", "helm",
            new Dictionary<string, object?> { ["heading"] = 180 });
        yield return Case("ws-helm", "  helm   heading   45  ", true, "helm.set-heading", "helm",
            new Dictionary<string, object?> { ["heading"] = 45 });
    }

    private static IEnumerable<ParseCase> EmptyAndContextFailureCases()
    {
        yield return Case("fail-empty", "   ", false, expectedFailure: ParseFailureCode.EmptyPrompt);
        yield return Case("fail-no-prefix-heading", "heading 270", false, expectedFailure: ParseFailureCode.UnknownContext);
        yield return Case("fail-unknown-context", "unknown scan", false, expectedFailure: ParseFailureCode.UnknownContext);
        yield return Case("fail-context-only", "helm", false, expectedFailure: ParseFailureCode.UnknownCommand);
        yield return Case("fail-alias-unknown", "xyz heading 1", false, expectedFailure: ParseFailureCode.UnknownContext);

        foreach (var verb in new[] { "fire", "repair", "hail", "scan", "heading 1", "warp 2" })
        {
            yield return Case(
                $"fail-noprefix-{verb.Replace(' ', '-')}",
                verb,
                false,
                expectedFailure: ParseFailureCode.UnknownContext);
        }
    }

    private static IEnumerable<ParseCase> UnknownCommandMatrix()
    {
        foreach (var ctx in new[] { "helm", "tactical", "engineering", "nav", "comms", "admin", "alpha", "beta" })
        {
            foreach (var junk in new[]
                     {
                         "foobar", "jump", "tea earl grey hot", "make it so", "engage",
                         "reverse polarity", "beam me up", "red alert"
                     })
            {
                yield return Case(
                    $"fail-unknown-{ctx}-{junk.Replace(' ', '-')}",
                    $"{ctx} {junk}",
                    false,
                    expectedFailure: ParseFailureCode.UnknownCommand);
            }
        }
    }

    private static IEnumerable<ParseCase> ArgumentFailureCases()
    {
        yield return Case("fail-missing-heading", "helm heading", false, expectedFailure: ParseFailureCode.MissingArgument);
        yield return Case("fail-missing-warp", "helm warp", false, expectedFailure: ParseFailureCode.MissingArgument);
        yield return Case("fail-missing-destination", "nav course", false, expectedFailure: ParseFailureCode.MissingArgument);
        yield return Case("fail-invalid-heading-word", "helm heading west", false, expectedFailure: ParseFailureCode.InvalidArgument);
        yield return Case("fail-invalid-warp-word", "helm warp fast", false, expectedFailure: ParseFailureCode.InvalidArgument);
        yield return Case("fail-extra-helm", "helm heading 90 extra", false, expectedFailure: ParseFailureCode.InvalidArgument);
    }

    private static IEnumerable<ParseCase> InvalidHeadingSweep()
    {
        foreach (var bad in new[] { "north", "south", "port", "starboard", "infinity", "NaN", "abc", "12.5" })
        {
            yield return Case(
                $"fail-heading-{bad.Replace('.', '-')}",
                $"helm heading {bad}",
                false,
                expectedFailure: ParseFailureCode.InvalidArgument);
        }
    }

    private static ParseCase Case(
        string id,
        string prompt,
        bool success,
        string? commandName = null,
        string? context = null,
        IReadOnlyDictionary<string, object?>? arguments = null,
        ParseFailureCode? expectedFailure = null,
        int? minimumCandidateCount = null) =>
        new(id, prompt, success, commandName, expectedFailure, context, arguments, minimumCandidateCount);
}
