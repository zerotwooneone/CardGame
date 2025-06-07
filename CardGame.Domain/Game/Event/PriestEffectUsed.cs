using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

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
    // Changed from RevealedCardType to specific card info
    public string RevealedCardId { get; }
    public CardRank RevealedCardRank { get; } // Keep type for convenience

    public PriestEffectUsed(Guid gameId, Guid priestPlayerId, Guid targetPlayerId, string revealedCardId, CardRank revealedCardRank, Guid? correlationId = null) // Updated constructor
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        PriestPlayerId = priestPlayerId;
        TargetPlayerId = targetPlayerId;
        RevealedCardId = revealedCardId; // Assign ID
        RevealedCardRank = revealedCardRank ?? throw new ArgumentNullException(nameof(revealedCardRank)); // Assign Type
    }
}