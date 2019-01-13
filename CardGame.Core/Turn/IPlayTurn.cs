using System;
using CardGame.Core.Card;

namespace CardGame.Core.Turn
{
    public interface IPlayTurn
    {
        KnownPlayerHand RevealHand(Guid targetId, CardValue targetHand);
    }
}