using Microsoft.AspNetCore.SignalR;

namespace CardGame.Web.SignalR;

/// <summary>
/// Provides the User ID for SignalR connections based on the custom "PlayerId" claim.
/// This allows targeting specific users via Clients.User(...) using their PlayerId Guid.
/// </summary>
public class PlayerIdUserIdProvider : IUserIdProvider
{
    /// <summary>
    /// Gets the user ID from the connection context.
    /// </summary>
    /// <param name="connection">The SignalR hub connection context.</param>
    /// <returns>The PlayerId claim value as a string, or null if the claim is not present.</returns>
    public virtual string? GetUserId(HubConnectionContext connection)
    {
        // Access the claims principal associated with the connection
        var user = connection.User;

        // Find the custom "PlayerId" claim added during login
        // Ensure the claim type string ("PlayerId") exactly matches what was used in AuthController
        return user?.Claims.FirstOrDefault(c => c.Type == "PlayerId")?.Value;

        // Alternative: Fallback to NameIdentifier if PlayerId claim is missing
        // return user?.FindFirstValue("PlayerId") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}