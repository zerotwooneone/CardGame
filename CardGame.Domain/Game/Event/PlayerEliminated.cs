using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Domain.Game.Event;

public record PlayerEliminated : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid PlayerId { get; }
    public string Reason { get; }
    public CardType? CardResponsible { get; }

    public PlayerEliminated(Guid gameId, Guid playerId, string reason, CardType? cardResponsible, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        PlayerId = playerId;
        Reason = reason ?? string.Empty;
        CardResponsible = cardResponsible;
    }
}