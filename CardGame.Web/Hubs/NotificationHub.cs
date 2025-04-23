using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CardGame.Web.Hubs;

/// <summary>
/// SignalR hub for sending general user notifications, like game invitations.
/// Requires users to be authenticated to connect.
/// </summary>
[Authorize] // Ensures only authenticated users can connect to this hub
public class NotificationHub : Hub<INotificationClient> // Specify client interface for strong typing
{
    private readonly ILogger<NotificationHub> _logger; // Logger instance

    // Constructor injection for ILogger
    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // --- Client-to-Server Methods (Optional) ---
    // Add methods here if clients need to invoke server logic via this hub.
    // Example: public async Task AcknowledgeNotification(Guid notificationId) { ... }

    // --- Server-to-Client Methods (Defined implicitly by SendAsync calls) ---
    // The server will call methods on clients like:
    // - ReceiveGameInvitation(Guid gameId, string gameCreatorName)

    // --- Hub Lifecycle Methods (Optional Overrides) ---
    public override async Task OnConnectedAsync()
    {
        // Optional: Logic when a user connects (e.g., logging)
        // The user's identity (Context.UserIdentifier) should be available here
        // if the UserIdProvider is configured correctly.
        var userId = Context.UserIdentifier ?? "anonymous"; // Handle potential null identifier
        _logger.LogInformation("NotificationHub: User connected: {UserId}", userId); // Use structured logging
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Optional: Logic when a user disconnects (e.g., logging)
        var userId = Context.UserIdentifier ?? "anonymous"; // Handle potential null identifier
        if (exception == null)
        {
            _logger.LogInformation("NotificationHub: User disconnected: {UserId}", userId);
        }
        else
        {
            // Log warning or error if disconnection was due to an exception
            _logger.LogWarning(exception, "NotificationHub: User disconnected with error: {UserId}", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Interface defining the client methods that the NotificationHub can invoke.
/// Useful for strongly-typed hub contexts.
/// </summary>
public interface INotificationClient
{
    Task ReceiveGameInvitation(Guid gameId, string gameCreatorName);
    // Add other notification methods here
}