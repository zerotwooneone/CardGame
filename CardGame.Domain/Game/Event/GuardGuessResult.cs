using CardGame.Domain.Interfaces;
using CardGame.Domain.Turn;

namespace CardGame.Domain.Game.Event;

public record GuardGuessResult : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid GuesserId { get; }
    public Guid TargetId { get; }
    public CardType GuessedCardType { get; }
    public bool WasCorrect { get; }

    public GuardGuessResult(Guid gameId, Guid guesserId, Guid targetId, CardType guessedCardType, bool wasCorrect, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        GuesserId = guesserId;
        TargetId = targetId;
        GuessedCardType = guessedCardType;
        WasCorrect = wasCorrect;
    }
}