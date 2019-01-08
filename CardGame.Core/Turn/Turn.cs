using System;
using CardGame.Core.Card;

namespace CardGame.Core.Turn
{
    public class Turn
    {
        public readonly Player.Player CurrentPlayer;
        public CardValue Unplayable { get; private set; }

        public KnownPlayerHand KnownPlayerHand {get; private set; }

        public Turn(Player.Player currentPlayer)
        {
            CurrentPlayer = currentPlayer;
            KnownPlayerHand = null;
        }

        public void MarkUnplayable(CardValue cardValue)
        {
            Unplayable = cardValue;
        }
        
        public void RevealHand(Guid targetId, CardValue value)
        {
            KnownPlayerHand = new KnownPlayerHand(targetId, value);
        }
    }
}