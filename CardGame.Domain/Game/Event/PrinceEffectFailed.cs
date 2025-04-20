using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record PrinceEffectFailed : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid TargetPlayerId { get; }
    public string Reason { get; }

    public PrinceEffectFailed(Guid gameId, Guid targetPlayerId, string reason, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        TargetPlayerId = targetPlayerId;
        Reason = reason ?? string.Empty;
    }
}