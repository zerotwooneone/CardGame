using System;
using CardGame.Core.Card;

namespace CardGame.Core.Turn
{
    public class Turn : IPlayTurn
    {
        public readonly Guid CurrentPlayerId;

        public Turn(Guid currentPlayerId, ushort id)
        {
            Id = id;
            CurrentPlayerId = currentPlayerId;
            KnownPlayerHand = null;
        }

        public ushort Id { get; }
        public CardValue Unplayable { get; private set; }

        public KnownPlayerHand KnownPlayerHand { get; private set; }

        public KnownPlayerHand PlayPriest(Guid targetId, CardValue targetHand)
        {
            KnownPlayerHand = new KnownPlayerHand(targetId, targetHand);
            return KnownPlayerHand;
        }

        public void MarkUnplayable(CardValue cardValue1, CardValue cardValue2)
        {
            if (cardValue1 == CardValue.Countess)
            {
                if (cardValue2 == CardValue.King || cardValue2 == CardValue.Prince) Unplayable = cardValue2;
            }
            else if (cardValue2 == CardValue.Countess)
            {
                if (cardValue1 == CardValue.King || cardValue1 == CardValue.Prince) Unplayable = cardValue1;
            }
        }
    }
}