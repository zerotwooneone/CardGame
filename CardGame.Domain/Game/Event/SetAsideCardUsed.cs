using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using CardRank = CardGame.Domain.BaseGame.CardRank;

namespace CardGame.Domain.Game.Event;

public record SetAsideCardUsed(Guid GameId, CardRank UsedCardRank) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public Guid? CorrelationId { get; init; }
    // Constructor needed if using init property outside record declaration
    public SetAsideCardUsed(Guid gameId, CardRank usedCardRank, Guid? correlationId = null) : this(gameId, usedCardRank) { CorrelationId = correlationId; }
}