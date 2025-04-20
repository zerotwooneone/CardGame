using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record TurnStarted : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid PlayerId { get; }
    public int RoundNumber { get; }

    public TurnStarted(Guid gameId, Guid playerId, int roundNumber, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        PlayerId = playerId;
        RoundNumber = roundNumber;
    }
}