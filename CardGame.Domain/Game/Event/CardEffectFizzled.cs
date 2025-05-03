using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Domain.Game;

/// <summary>
/// Raised when a card effect fails to apply, often due to Handmaid protection.
/// </summary>
public record CardEffectFizzled : IDomainEvent // Implement IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid ActorId { get; } // Player who played the card
    public CardType CardType { get; } // The card whose effect fizzled
    public Guid TargetId { get; } // The player targeted
    public string Reason { get; } // e.g., "Target was protected"

    public CardEffectFizzled(Guid gameId, Guid actorId, CardType cardType, Guid targetId, string reason, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        ActorId = actorId;
        CardType = cardType ?? throw new ArgumentNullException(nameof(cardType));
        TargetId = targetId;
        Reason = reason ?? string.Empty;
    }
}