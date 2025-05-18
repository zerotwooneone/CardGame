using CardGame.Application.Common.Interfaces; // For IDeckRepository
using CardGame.Domain.Common; // For IDomainEventPublisher
using CardGame.Domain.Interfaces; // For IGameRepository, IDeckProvider, IDeckRegistry, IDeckProviderRegistrar
using CardGame.Infrastructure.Persistence;
using CardGame.Infrastructure.Services; // For DeckRegistry (impl), ServiceCollectionDeckProviderAdapter, CoreDeckProviderRegistrar (impl)
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Infrastructure;

/// <summary>
/// Static class for configuring infrastructure layer services for dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration) // Pass configuration if needed
    {
        // --- Persistence ---
        services.AddScoped<IGameRepository, InMemoryGameRepository>(); // IGameRepository from Domain.Interfaces
        services.AddScoped<IDeckRepository, DeckRepository>(); // IDeckRepository from Application.Common.Interfaces

        // --- Deck Providers and Registry ---
        // Discover and register deck providers from assemblies implementing IDeckProviderRegistrar.
        var registrarType = typeof(IDeckProviderRegistrar); // This is now CardGame.Domain.Interfaces.IDeckProviderRegistrar
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var registrarTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(p => registrarType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
            .ToList();

        var providerAdapter = new ServiceCollectionDeckProviderAdapter(services);
        foreach (var type in registrarTypes)
        {
            if (Activator.CreateInstance(type) is IDeckProviderRegistrar registrar)
            {
                registrar.RegisterDeckProviders(providerAdapter);
            }
        }
        // Manual registration for core providers can still be done here if they don't come from a registrar,
        // or they can have their own registrar within this project.
        // For example, we can create a CoreDeckProviderRegistrar for DefaultDeckProvider.

        // Register the DeckRegistry as a singleton. It holds the collection of providers registered via the adapter.
        services.AddSingleton<IDeckRegistry, DeckRegistry>(); // IDeckRegistry from Domain.Interfaces, DeckRegistry from Infrastructure.Services

        // --- Domain Services / Helpers ---

        // Register the default randomizer (could be Singleton or Transient)
        services.AddSingleton<IRandomizer, DefaultRandomizer>();

        // Register the implementation for the domain event publisher
        // Assuming MediatRDomainEventPublisher exists and implements IDomainEventPublisher
        // Transient might be suitable if it only depends on Scoped/Transient services like IMediator
        // services.AddTransient<IDomainEventPublisher, MediatRDomainEventPublisher>();
        
        
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        services.AddScoped<IDomainEventPublisher, MediatRDomainEventPublisher>();

        return services;
    }
}