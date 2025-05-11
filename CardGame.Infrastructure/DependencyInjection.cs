using CardGame.Application.Common.Interfaces;
using CardGame.Domain.Common;
using CardGame.Domain.Interfaces;
using CardGame.Infrastructure.Persistance;
using CardGame.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Infrastructure;

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

        // Register the InMemory repository as a Singleton since its internal storage is static
        // When you switch to a real DB, you'd register your EF Core context (usually Scoped)
        // and the real repository implementation (usually Scoped).
        services.AddSingleton<IGameRepository, InMemoryGameRepository>();
        services.AddSingleton<IDeckRepository, DeckRepository>();

        // Example for EF Core (when you add it later):
        /*
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))); // Or UseNpgsql, etc.

        services.AddScoped<IGameRepository, EfGameRepository>(); // Register the EF Core implementation
        */


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