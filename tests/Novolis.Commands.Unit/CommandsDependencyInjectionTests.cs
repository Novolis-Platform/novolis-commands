using Microsoft.Extensions.DependencyInjection;
using Novolis.Commands.Abstractions;
using Novolis.Commands.DependencyInjection;
using Novolis.Commands.Engine;
using Novolis.Commands.Queueing;

namespace Novolis.Commands.Unit;

public sealed class CommandsDependencyInjectionTests
{
    private sealed class Ctx;

    private sealed class Resolver : ICommandContextResolver<Ctx>
    {
        public IReadOnlyDictionary<string, string> GetContextAliases(Ctx context) =>
            new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> GetVerbAliases(Ctx context) =>
            new Dictionary<string, string>();
    }

    [Test]
    public async Task AddNovolisCommands_RegistersEngineAndQueue()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICommandContextResolver<Ctx>, Resolver>();
        services.AddNovolisCommands<Ctx>();

        await using var sp = services.BuildServiceProvider();
        await Assert.That(sp.GetService<ICommandRegistry>()).IsNotNull();
        await Assert.That(sp.GetService<ICommandQueue>()).IsNotNull();
        await Assert.That(sp.GetService<ICommandEngine<Ctx>>()).IsNotNull();
        await Assert.That(sp.GetService<BuiltInCommandMatcher>()).IsNotNull();
    }

    [Test]
    public async Task AddNovolisCommands_NullServices_Throws()
    {
        await Assert.That(() => ServiceCollectionExtensions.AddNovolisCommands<object>(null!))
            .Throws<ArgumentNullException>();
    }
}
