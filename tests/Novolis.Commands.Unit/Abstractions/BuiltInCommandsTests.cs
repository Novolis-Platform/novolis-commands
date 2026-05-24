using TUnit.Core;

namespace Novolis.Commands.Abstractions.Tests;

public sealed class BuiltInCommandsTests
{
    public static IEnumerable<string> AllBuiltIns() =>
    [
        BuiltInCommands.BelayThat,
        BuiltInCommands.ClearQueue,
        BuiltInCommands.RepeatLast,
        BuiltInCommands.Help
    ];

    [Test]
    [MethodDataSource(nameof(AllBuiltIns))]
    public async Task BuiltIn_Name_Should_Start_With_System_Prefix(string name)
    {
        await Assert.That(name.StartsWith("system.", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task BuiltIn_Names_Should_Be_Unique()
    {
        var names = AllBuiltIns().ToArray();
        await Assert.That(names.Distinct(StringComparer.Ordinal).Count()).IsEqualTo(names.Length);
    }
}
