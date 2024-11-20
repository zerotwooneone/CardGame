using CardGame.Domain.Turn;

namespace CardGame.Application;

public class CardEffectRepository : ICardEffectRepository
{
    public Task<CardEffect?> Get(GameId gameId, CardId cardId, PlayParams playParams)
    {
        var card = Cards.AllCards.FirstOrDefault(c=> c.Id == cardId);
        if (card == null)
        {
            return Task.FromException<CardEffect?>(new Exception("card not found"));
        }
        var isPrincess = card.Value == CardValues.Princess;
        var isKing = card.Value == CardValues.King;
        var isPrince = card.Value == CardValues.Prince;
        var isHandmaid = card.Value == CardValues.Handmaid;
        var isPriest = card.Value == CardValues.Priest;
        var isBaron = card.Value == CardValues.Baron;
        var isGuard = card.Value == CardValues.Guard;
        //var isCountess = card.Value == CardValues.Countess;
        return Task.FromResult<CardEffect?>(new CardEffect
        {
            KickOutOfRoundOnDiscard = isPrincess,
            CanDiscard = true,
            CanTargetSelf = isPrince,
            Card = cardId,
            Compare = isBaron,
            DiscardAndDraw = isPrince,
            DiscardAndDrawKickEnabled = true,
            Guess = isGuard,
            Inspect = isPriest,
            Protect = isHandmaid,
            TradeHands = isKing,
            RequiresTargetPlayer = isKing || isPrince || isBaron || isPriest || isGuard
        });
    }
}