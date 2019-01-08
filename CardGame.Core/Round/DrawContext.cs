using System;
using CardGame.Core.Card;

namespace CardGame.Core.Round
{
    public class DrawContext
    {
        private readonly Func<Hand.Hand> _getCurrentPlayerHand;

        public DrawContext(Turn.Turn currentTurn,
            Func<Hand.Hand> getCurrentPlayerHand)
        {
            CurrentTurn = currentTurn;
            _getCurrentPlayerHand = getCurrentPlayerHand;
        }

        public Turn.Turn CurrentTurn { get; }

        public void MarkUnplayableCards()
        {
            var hand = _getCurrentPlayerHand();
            if (hand.Previous.Value == CardValue.Countess && (
                    hand.Drawn.Value == CardValue.King ||
                    hand.Drawn.Value == CardValue.Prince))
                CurrentTurn.MarkUnplayable(hand.Drawn.Value);
            else if (hand.Drawn?.Value == CardValue.Countess && (
                         hand.Previous.Value == CardValue.King ||
                         hand.Previous.Value == CardValue.Prince))
                CurrentTurn.MarkUnplayable(hand.Previous.Value);
        }
    }
}