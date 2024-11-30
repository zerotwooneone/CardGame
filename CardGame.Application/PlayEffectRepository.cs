using CardGame.Domain.Turn;

namespace CardGame.Application;

public class PlayEffectRepository : IPlayEffectRepository, IForcedDiscardEffectRepository
{
    private readonly IReadOnlyCollection<CardValue> _countessForcedValues = new CardValue[] { CardValues.King, CardValues.Prince };
    public Task<PlayableCard?> Get(GameId gameId, CardId cardId, PlayParams playParams)
    {
        var card = Cards.AllCards.FirstOrDefault(c=> c.Id == cardId);
        if (card == null)
        {
            return Task.FromException<PlayableCard?>(new Exception("card not found"));
        }
        var isPrincess = card.Value == CardValues.Princess;
        var isKing = card.Value == CardValues.King;
        var isPrince = card.Value == CardValues.Prince;
        var isHandmaid = card.Value == CardValues.Handmaid;
        var isPriest = card.Value == CardValues.Priest;
        var isBaron = card.Value == CardValues.Baron;
        var isGuard = card.Value == CardValues.Guard;
        var isCountess = card.Value == CardValues.Countess;
        
        return Task.FromResult<PlayableCard?>(new PlayableCard
        {
            KickOutOfRoundOnDiscard = isPrincess,
            PlayProhibitedByCardInHand = isCountess
                ? _countessForcedValues
                : Array.Empty<CardValue>(),
            CanTargetSelf = isPrince,
            CardId = cardId,
            Value = card.Value,
            Compare = isBaron,
            DiscardAndDraw = isPrince,
            Guess = isGuard,
            Inspect = isPriest,
            Protect = isHandmaid,
            TradeHands = isKing,
            RequiresTargetPlayer = isKing || isPrince || isBaron || isPriest || isGuard
        });
    }

    public bool DiscardAndDrawKickEnabled => true;
    public Task<ForcedDiscardEffect?> Get(CardValue value)
    {
        var isPrincess = value == CardValues.Princess;
        return Task.FromResult<ForcedDiscardEffect?>(new ForcedDiscardEffect
        {
            KickOutOfRoundOnDiscard = isPrincess,
            DiscardAndDrawKickEnabled = DiscardAndDrawKickEnabled,
        });
    }
}

