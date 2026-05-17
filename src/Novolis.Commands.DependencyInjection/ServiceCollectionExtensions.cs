using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Commands.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers command registry, queue, built-in matcher, and <see cref="ICommandEngine{TContext}"/>.
    /// The host must register <see cref="ICommandContextResolver{TContext}"/> separately.
    /// </summary>
    public static IServiceCollection AddNovolisCommands<TContext>(
        this IServiceCollection services,
        Action<CommandRegistryBuilder>? configureRegistry = null,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : notnull =>
        AddNovolisCommands<TContext>(services, configureRegistry, configureEngine: null, lifetime);

    /// <summary>
    /// Registers command services and optional <see cref="CommandEngineOptions"/> (argument parsers, built-ins).
    /// </summary>
    public static IServiceCollection AddNovolisCommands<TContext>(
        this IServiceCollection services,
        Action<CommandRegistryBuilder>? configureRegistry,
        Action<CommandEngineOptions>? configureEngine,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : notnull
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new CommandRegistryBuilder();
        configureRegistry?.Invoke(builder);

        var engineOptions = new CommandEngineOptions();
        configureEngine?.Invoke(engineOptions);

        services.Add(new ServiceDescriptor(typeof(ICommandRegistry), _ => builder.Build(), lifetime));
        services.Add(new ServiceDescriptor(typeof(ICommandQueue), typeof(ChannelCommandQueue), lifetime));
        services.Add(new ServiceDescriptor(typeof(BuiltInCommandMatcher), typeof(BuiltInCommandMatcher), lifetime));
        services.Add(new ServiceDescriptor(typeof(CommandEngineOptions), _ => engineOptions, lifetime));
        services.Add(new ServiceDescriptor(
            typeof(ICommandEngine<TContext>),
            sp => new CommandEngine<TContext>(
                sp.GetRequiredService<ICommandRegistry>(),
                sp.GetRequiredService<ICommandContextResolver<TContext>>(),
                sp.GetRequiredService<CommandEngineOptions>()),
            lifetime));

        return services;
    }

    /// <summary>
    /// Registers <see cref="CommandQueueRunner{TContext}"/>. The host must register <see cref="ICommandProcessor{TContext}"/>.
    /// </summary>
    public static IServiceCollection AddNovolisCommandRunner<TContext>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : notnull
    {
        ArgumentNullException.ThrowIfNull(services);
        services.Add(new ServiceDescriptor(
            typeof(CommandQueueRunner<TContext>),
            typeof(CommandQueueRunner<TContext>),
            lifetime));
        return services;
    }
}
