namespace CardGame.Application.Common.Interfaces;

/// <summary>
/// Interface for retrieving the current user's identity.
/// Defined in Application layer as it's a contract needed by Application/Web layers.
/// </summary>
public interface IUserAuthenticationService
{
    /// <summary>
    /// Gets the ID of the currently authenticated user.
    /// Returns Guid.Empty if no user is authenticated.
    /// </summary>
    Guid GetCurrentUserId();
}