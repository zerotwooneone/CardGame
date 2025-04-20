using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record PlayerDrewCard : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid PlayerId { get; }

    public PlayerDrewCard(Guid gameId, Guid playerId, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        PlayerId = playerId;
    }
}