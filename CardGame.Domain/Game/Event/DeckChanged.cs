using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record DeckChanged : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public int CardsRemaining { get; }

    public DeckChanged(Guid gameId, int cardsRemaining, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        CardsRemaining = cardsRemaining;
    }
}