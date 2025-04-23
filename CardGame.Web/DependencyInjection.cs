using System.Reflection;
using CardGame.Application.GameEventHandlers;
using CardGame.Web.SignalR;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace CardGame.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddCardGameWebServices(this IServiceCollection services)
    {
        services.AddSingleton<INotificationService, SignalRNotificationService>();
        
        // This tells SignalR to use the "PlayerId" claim from the authenticated user's
        // identity as the UserIdentifier for targeting specific users.
        services.AddSingleton<IUserIdProvider, PlayerIdUserIdProvider>();

        return services;
    }
}