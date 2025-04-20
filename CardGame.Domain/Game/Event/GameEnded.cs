using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record GameEnded : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid WinnerPlayerId { get; }

    public GameEnded(Guid gameId, Guid winnerPlayerId, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        WinnerPlayerId = winnerPlayerId;
    }
}