using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record RoundStarted : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public int RoundNumber { get; }
    public List<Guid> PlayerIds { get; }
    public int DeckCount { get; }
    public Card? SetAsideCard { get; }
    public List<PublicCardInfo> PubliclySetAsideCards { get; }
    public Guid DeckId { get; }

    public RoundStarted(Guid gameId, int roundNumber, List<Guid> playerIds, int deckCount, Card? setAsideCard,
        List<PublicCardInfo> publiclySetAsideCards, Guid deckId, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        RoundNumber = roundNumber;
        PlayerIds = playerIds ?? new List<Guid>();
        DeckCount = deckCount;
        SetAsideCard = setAsideCard;
        PubliclySetAsideCards = publiclySetAsideCards;
        DeckId = deckId;
    }
}