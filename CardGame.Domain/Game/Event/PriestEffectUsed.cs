using CardGame.Domain.Interfaces;
using CardGame.Domain.Turn;

namespace CardGame.Domain.Game.Event;

public record PriestEffectUsed : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid PriestPlayerId { get; }
    public Guid TargetPlayerId { get; }
    public CardType RevealedCardType { get; }

    public PriestEffectUsed(Guid gameId, Guid priestPlayerId, Guid targetPlayerId, CardType revealedCardType, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        PriestPlayerId = priestPlayerId;
        TargetPlayerId = targetPlayerId;
        RevealedCardType = revealedCardType;
    }
}