using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

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
    public CardRank? SetAsideCardType { get; }
    public List<PublicCardInfo> PubliclySetAsideCards { get; }
    public Guid DeckId { get; }

    public RoundStarted(Guid gameId, int roundNumber, List<Guid> playerIds, int deckCount, CardRank? setAsideCardType,
        List<PublicCardInfo> publiclySetAsideCards, Guid deckId, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        RoundNumber = roundNumber;
        PlayerIds = playerIds ?? new List<Guid>();
        DeckCount = deckCount;
        SetAsideCardType = setAsideCardType;
        PubliclySetAsideCards = publiclySetAsideCards;
        DeckId = deckId;
    }
}