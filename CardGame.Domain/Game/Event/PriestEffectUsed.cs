using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record PriestEffectUsed : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }
    public Guid GameId { get; }
    public Guid PriestPlayerId { get; }
    public Guid TargetPlayerId { get; }
    public string RevealedCardId { get; }
    public Card RevealedCard { get; } 

    public PriestEffectUsed(Guid gameId, Guid priestPlayerId, Guid targetPlayerId, string revealedCardId, Card revealedCard, Guid? correlationId = null) // Updated constructor
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        PriestPlayerId = priestPlayerId;
        TargetPlayerId = targetPlayerId;
        RevealedCardId = revealedCardId; // Assign ID
        RevealedCard = revealedCard ?? throw new ArgumentNullException(nameof(revealedCard)); // Assign Type
    }
}