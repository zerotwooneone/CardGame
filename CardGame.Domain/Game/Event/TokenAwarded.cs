using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record TokenAwarded : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid PlayerId { get; }
    public int NewTokenCount { get; }

    public TokenAwarded(Guid gameId, Guid playerId, int newTokenCount, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        PlayerId = playerId;
        NewTokenCount = newTokenCount;
    }
}