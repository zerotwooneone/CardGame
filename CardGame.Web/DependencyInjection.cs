using CardGame.Application.Common.Interfaces;
using CardGame.Application.GameEventHandlers;
using CardGame.Web.Hubs;
using CardGame.Web.SignalR;
using Microsoft.AspNetCore.SignalR;

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

        services.AddSingleton<IGameStateBroadcaster, SignalRGameStateBroadcaster>();
        services.AddSingleton<IPlayerNotifier,SignalRPlayerNotifier>();

        return services;
    }
}