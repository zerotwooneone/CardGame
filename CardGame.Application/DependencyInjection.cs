using System.Reflection;
using CardGame.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Providers;

namespace CardGame.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register MediatR - scans the current assembly (LoveLetter.Application)
        // for IRequestHandlers, INotificationHandlers, etc.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly()); // Add this line

        // Register any other application-specific services here (e.g., interfaces/implementations)
        // services.AddScoped<ISomeApplicationService, SomeApplicationService>();

        // Register application-specific services
        services.AddScoped<GameStateUpdateService>();

        return services;
    }
}