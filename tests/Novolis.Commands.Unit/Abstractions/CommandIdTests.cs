using TUnit.Core;

namespace Novolis.Commands.Abstractions.Tests;

public sealed class CommandIdTests
{
    public static IEnumerable<int> BatchSizes() => Enumerable.Range(1, 50);

    [Test]
    [MethodDataSource(nameof(BatchSizes))]
    public async Task New_Should_Produce_Unique_Ids(int _)
    {
        var ids = Enumerable.Range(0, 32).Select(_ => CommandId.New()).ToArray();
        var distinct = ids.Select(id => id.Value).Distinct().Count();
        await Assert.That(distinct).IsEqualTo(ids.Length);
    }

    [Test]
    public async Task New_Should_Not_Be_Empty_Guid()
    {
        var id = CommandId.New();
        await Assert.That(id.Value).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task Equality_Should_Use_Guid_Value()
    {
        var guid = Guid.CreateVersion7();
        var a = new CommandId(guid);
        var b = new CommandId(guid);
        await Assert.That(a).IsEqualTo(b);
    }
}
