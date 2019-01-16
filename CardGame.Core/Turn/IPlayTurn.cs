using System;
using CardGame.Core.Card;

namespace CardGame.Core.Turn
{
    public interface IPlayTurn
    {
        KnownPlayerHand PlayPriest(Guid targetId, CardValue targetHand);
    }
}