using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record PrinceEffectUsed : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid ActorPlayerId { get; }
    public Guid TargetPlayerId { get; }
    public Card DiscardedCard { get; }
    public string DiscardedCardId { get; }

    public PrinceEffectUsed(Guid gameId, Guid actorPlayerId, Guid targetPlayerId, Card discardedCard, string discardedCardId, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        ActorPlayerId = actorPlayerId;
        TargetPlayerId = targetPlayerId;
        DiscardedCard = discardedCard;
        DiscardedCardId = discardedCardId;
    }
}