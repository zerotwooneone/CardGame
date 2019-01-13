using System;
using CardGame.Core.Card;

namespace CardGame.Core.Turn
{
    public class Turn
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

        public KnownPlayerHand RevealHand(Guid targetId, CardValue value)
        {
            KnownPlayerHand = new KnownPlayerHand(targetId, value);
            return KnownPlayerHand;
        }
    }
}