﻿using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;

namespace CardGame.Domain.Game.Event;

public record PrinceEffectUsed : IDomainEvent
{
    public Guid EventId { get; }
    public DateTimeOffset OccurredOn { get; }
    public Guid? CorrelationId { get; init; }

    // Event Specific Properties
    public Guid GameId { get; }
    public Guid ActorPlayerId { get; }
    public Guid TargetPlayerId { get; }
    public CardType DiscardedCardType { get; }
    public string DiscardedCardId { get; }

    public PrinceEffectUsed(Guid gameId, Guid actorPlayerId, Guid targetPlayerId, CardType discardedCardType, string discardedCardId, Guid? correlationId = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTimeOffset.UtcNow;
        CorrelationId = correlationId;
        GameId = gameId;
        ActorPlayerId = actorPlayerId;
        TargetPlayerId = targetPlayerId;
        DiscardedCardType = discardedCardType;
        DiscardedCardId = discardedCardId;
    }
}