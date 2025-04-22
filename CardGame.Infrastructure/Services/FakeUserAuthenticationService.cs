using CardGame.Application.Common.Interfaces;

namespace CardGame.Infrastructure.Services;

// Define known player IDs for testing/simulation
public static class FakePlayerIds
{
    public static readonly Guid Alice = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
    public static readonly Guid Bob = new Guid("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
    public static readonly Guid Charlie = new Guid("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC");
    public static readonly Guid Diana = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD");

    // You might want to ensure these IDs match players created in your
    // InMemoryGameRepository or other test data setup.
}

/// <summary>
/// Fake implementation that always returns a hardcoded user ID (e.g., Alice).
/// </summary>
public class FakeUserAuthenticationService : IUserAuthenticationService
{
    private readonly Guid _hardcodedUserId;

    public FakeUserAuthenticationService()
    {
        // Simulate that Alice is always the logged-in user for this fake service
        _hardcodedUserId = FakePlayerIds.Alice;
    }

    // In a real scenario, this method would inspect HttpContext.User, validate tokens, etc.
    public Guid GetCurrentUserId()
    {
        // Always return Alice's ID for this fake implementation
        return _hardcodedUserId;
    }
}