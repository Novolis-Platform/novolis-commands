using Novolis.Commands.Engine;

namespace Novolis.Commands.Engine.Tests.Support;

public static class AmbiguousCommandRegistry
{
    public static ICommandRegistry Create() =>
        new CommandRegistryBuilder()
            .Add("dup.a", "shared", ["scan"])
            .Add("dup.b", "shared", ["scan"])
            .Add("dup.c", "shared", ["lock target"])
            .Add("dup.d", "shared", ["lock target"])
            .Build(validate: false);
}
