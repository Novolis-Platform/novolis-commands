using Novolis.Commands.Engine;
using Novolis.Commands.Engine.Tests.Support;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class HeadingArgumentParserTests
{
    private static readonly TestHeading3dArgumentParser Parser = new();
    private static readonly CommandDefinition Definition = new(
        "helm.set-heading",
        "helm",
        ["heading"],
        [],
        "heading3d");

    public static IEnumerable<(string[] Tokens, double Heading, double? HeadingBy)> Cases() =>
    [
        (["270"], 270, null),
        (["122", "by", "180"], 122, 180),
        (["122", "mark", "6", "by", "180"], 122.6, 180),
        (["to", "122", "by", "180"], 122, 180),
        (["set", "heading", "to", "122", "mark", "6", "by", "180", "mark", "2"], 122.6, 180.2),
        (["123,5", "by", "119,4"], 123.5, 119.4),
        (["123.5", "by", "119.4"], 123.5, 119.4)
    ];

    [Test]
    [MethodDataSource(nameof(Cases))]
    public async Task TryParse_Should_Parse_3D_Heading((string[] Tokens, double Heading, double? HeadingBy) data)
    {
        var ok = Parser.TryParse(Definition, data.Tokens, out var arguments, out var failure);

        await Assert.That(ok).IsTrue();
        await Assert.That(failure).IsNull();
        await Assert.That(arguments["heading"]).IsEqualTo(data.Heading);
        if (data.HeadingBy is double expectedBy)
            await Assert.That(arguments["headingBy"]).IsEqualTo(expectedBy);
        else
            await Assert.That(arguments.ContainsKey("headingBy")).IsFalse();
    }

    [Test]
    public async Task TryParse_Should_Reject_Invalid_Tokens()
    {
        var ok = Parser.TryParse(Definition, ["west"], out _, out var failure);
        await Assert.That(ok).IsFalse();
        await Assert.That(failure).IsNotNull();
    }
}
