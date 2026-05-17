using Novolis.Commands.Engine;
using TUnit.Core;

namespace Novolis.Commands.Engine.Tests;

public sealed class HeadingArgumentParserTests
{
    public static IEnumerable<(string[] Tokens, double Heading, double? HeadingBy)> Cases() =>
    [
        (["270"], 270, null),
        (["122", "by", "180"], 122, 180),
        (["122", "mark", "6", "by", "180"], 122.6, 180),
        (["to", "122", "by", "180"], 122, 180),
        (["set", "heading", "to", "122", "mark", "6", "by", "180", "mark", "2"], 122.6, 180.2)
    ];

    [Test]
    [MethodDataSource(nameof(Cases))]
    public async Task TryParse_Should_Parse_3D_Heading((string[] Tokens, double Heading, double? HeadingBy) data)
    {
        var ok = HeadingArgumentParser.TryParse(data.Tokens, out var heading, out var headingBy);

        await Assert.That(ok).IsTrue();
        await Assert.That(heading).IsEqualTo(data.Heading);
        if (data.HeadingBy is double expectedBy)
            await Assert.That(headingBy).IsEqualTo(expectedBy);
        else
            await Assert.That(headingBy).IsNull();
    }

    [Test]
    public async Task TryParse_Should_Reject_Invalid_Tokens()
    {
        var ok = HeadingArgumentParser.TryParse(["west"], out _, out _);
        await Assert.That(ok).IsFalse();
    }
}
