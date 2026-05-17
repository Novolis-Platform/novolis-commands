using Novolis.Commands.Engine;
using Novolis.Commands.Engine.Tests.Support;
using Novolis.Commands.Testing;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class NaturalOrderTests
{
    private static CommandEngine<TestCommandContext> CreateEngine() =>
        CommandEngineTestSupport.CreateBridge();

    public static IEnumerable<(string Prompt, string Command, Dictionary<string, object?>? Args)> LogLines() =>
    [
        ("helm come about", "helm.come-about", null),
        ("helm all ahead full", "helm.all-ahead-full", null),
        ("weaps target the closest enemy", "tactical.lock-target", null),
        (
            "helm, set heading to 122 by 180",
            "helm.set-heading",
            new Dictionary<string, object?> { ["heading"] = 122.0, ["headingBy"] = 180.0 }),
        (
            "helm, set course 123,5 by 119,4",
            "helm.set-heading",
            new Dictionary<string, object?> { ["heading"] = 123.5, ["headingBy"] = 119.4 }),
        (
            "helm set course 123.5 by 119.4",
            "helm.set-heading",
            new Dictionary<string, object?> { ["heading"] = 123.5, ["headingBy"] = 119.4 }),
        (
            "helm course 122 by 33",
            "helm.set-heading",
            new Dictionary<string, object?> { ["heading"] = 122.0, ["headingBy"] = 33.0 })
    ];

    [Test]
    [MethodDataSource(nameof(LogLines))]
    public async Task Parse_Log_Lines_Should_Succeed((string Prompt, string Command, Dictionary<string, object?>? Args) data)
    {
        var engine = CreateEngine();
        var result = await engine.ParseCommandAsync(data.Prompt, TestContexts.Bridge);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Command!.Name).IsEqualTo(data.Command);

        if (data.Command == "helm.set-heading")
        {
            await Assert.That(result.Command.ContextWord).IsEqualTo("helm");
        }

        if (data.Args is null)
            return;

        foreach (var (key, expected) in data.Args)
            await Assert.That(Convert.ToDouble(result.Command.Arguments[key])).IsEqualTo(Convert.ToDouble(expected));
    }
}
