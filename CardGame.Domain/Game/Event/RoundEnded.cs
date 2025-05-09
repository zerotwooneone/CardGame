using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Domain.Game.Event;

public record RoundEnded : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid? WinnerPlayerId { get; }
    public string Reason { get; }
    // Detailed player state at round end
    public List<PlayerRoundEndSummary> PlayerSummaries { get; }

    public RoundEnded(
        Guid gameId,
        Guid? winnerPlayerId,
        string reason,
        List<PlayerRoundEndSummary> playerSummaries, // Changed parameter
        Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        WinnerPlayerId = winnerPlayerId;
        Reason = reason ?? string.Empty;
        PlayerSummaries = playerSummaries ?? new List<PlayerRoundEndSummary>(); // Assign new parameter
    }
}