using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Commands.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovolisCommands<TContext>(
        this IServiceCollection services,
        Action<CommandRegistryBuilder>? configureRegistry = null,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : notnull
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new CommandRegistryBuilder();
        configureRegistry?.Invoke(builder);

        services.Add(new ServiceDescriptor(typeof(ICommandRegistry), _ => builder.Build(), lifetime));
        services.Add(new ServiceDescriptor(typeof(ICommandQueue), typeof(ChannelCommandQueue), lifetime));
        services.Add(new ServiceDescriptor(typeof(BuiltInCommandMatcher), typeof(BuiltInCommandMatcher), lifetime));
        services.Add(new ServiceDescriptor(
            typeof(ICommandEngine<TContext>),
            typeof(CommandEngine<TContext>),
            lifetime));

        return services;
    }
}
