using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using CardRank = CardGame.Domain.BaseGame.CardRank;

namespace CardGame.Domain.Game.Event;

public record BaronComparisonResult : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid Player1Id { get; }
    public CardRank Player1Card { get; }
    public Guid Player2Id { get; }
    public CardRank Player2Card { get; }
    public Guid? LoserPlayerId { get; }

    public BaronComparisonResult(Guid gameId, Guid player1Id, CardRank player1Card, Guid player2Id, CardRank player2Card, Guid? loserPlayerId, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        Player1Id = player1Id;
        Player1Card = player1Card;
        Player2Id = player2Id;
        Player2Card = player2Card;
        LoserPlayerId = loserPlayerId;
    }
}