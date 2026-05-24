using Novolis.Commands.Engine;
using Novolis.Commands.Testing;

namespace Novolis.Commands.Engine.Tests.Support;

internal static class CommandEngineTestSupport
{
    public static CommandEngine<TestCommandContext> Create(
        ICommandRegistry? registry = null) =>
        new(registry ?? CommandEngineTestRegistry.Create(), new TestCommandContextResolver());

    public static CommandEngine<TestCommandContext> CreateBridge() =>
        CreateBridge(CommandEngineTestRegistry.CreateBridge());

    public static CommandEngine<TestCommandContext> CreateBridge(ICommandRegistry registry)
    {
        var options = new CommandEngineOptions();
        options.ArgumentParsers.Register("heading3d", new TestHeading3dArgumentParser());
        return new CommandEngine<TestCommandContext>(registry, new TestCommandContextResolver(), options);
    }
}
