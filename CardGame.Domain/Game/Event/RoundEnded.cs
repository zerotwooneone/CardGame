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
    public Dictionary<Guid, CardType?> FinalHands { get; }
    public string Reason { get; }

    public RoundEnded(Guid gameId, Guid? winnerPlayerId, Dictionary<Guid, CardType?> finalHands, string reason, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        WinnerPlayerId = winnerPlayerId;
        FinalHands = finalHands ?? new Dictionary<Guid, CardType?>();
        Reason = reason ?? string.Empty;
    }
}