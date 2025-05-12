using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using CardGame.Domain.Game;

namespace CardGame.Domain.Game.Event;

public record PlayerPlayedCard : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid PlayerId { get; }
    public Card PlayedCard { get; }
    public Guid? TargetPlayerId { get; }
    public CardType? GuessedCardType { get; }

    public PlayerPlayedCard(Guid gameId, Guid playerId, Card playedCard, Guid? targetPlayerId, CardType? guessedCardType, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        PlayerId = playerId;
        PlayedCard = playedCard ?? throw new ArgumentNullException(nameof(playedCard));
        TargetPlayerId = targetPlayerId;
        GuessedCardType = guessedCardType;
    }
}