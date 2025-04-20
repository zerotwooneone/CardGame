using CardGame.Domain.Interfaces;

namespace CardGame.Domain.Game.Event;

public record GameCreated : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public List<PlayerInfo> Players { get; } // Assumes PlayerInfo record exists
    public int TokensToWin { get; }

    public GameCreated(Guid gameId, List<PlayerInfo> players, int tokensToWin, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        Players = players ?? new List<PlayerInfo>();
        TokensToWin = tokensToWin;
    }
}