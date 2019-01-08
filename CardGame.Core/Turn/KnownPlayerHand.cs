using System;
using CardGame.Core.Card;

namespace CardGame.Core.Turn
{
    public class KnownPlayerHand
    {
        public KnownPlayerHand(Guid playerId, CardValue cardValue)
        {
            PlayerId = playerId;
            CardValue = cardValue;
        }

        public Guid PlayerId { get; }
        public CardValue CardValue { get; }
    }
}