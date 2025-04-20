using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record KingEffectUsed : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid Player1Id { get; }
    public Guid Player2Id { get; }

    public KingEffectUsed(Guid gameId, Guid player1Id, Guid player2Id, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        Player1Id = player1Id;
        Player2Id = player2Id;
    }
}