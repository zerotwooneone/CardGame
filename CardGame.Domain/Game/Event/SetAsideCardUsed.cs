using CardGame.Domain.Interfaces;
using CardGame.Domain.Turn;

namespace CardGame.Domain.Game.Event;

public record SetAsideCardUsed(Guid GameId, CardType UsedCardType) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public Guid? CorrelationId { get; init; }
    // Constructor needed if using init property outside record declaration
    public SetAsideCardUsed(Guid gameId, CardType usedCardType, Guid? correlationId = null) : this(gameId, usedCardType) { CorrelationId = correlationId; }
}