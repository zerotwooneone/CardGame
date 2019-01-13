using System;
using CardGame.Core.Card;

namespace CardGame.Core.Round
{
    public class PlayContext
    {
        private readonly Action<Guid> _addPlayerProtection;
        private readonly Action<Guid> _eliminatePlayer;
        private readonly Action<Guid, Guid> _tradeHands;
        private readonly Func<Guid, CardValue> _revealHand;
        private readonly Action<Guid> _discardAndDraw;

        public PlayContext(Action<Guid> addPlayerProtection,
            Action<Guid> eliminatePlayer, Turn.Turn currentTurn,
            Action<Guid, Guid> tradeHands,
            Func<Guid, CardValue> revealHand,
            Action<Guid> discardAndDraw, 
            Guid? targetPlayerId, 
            CardValue? guessedCardvalue)
        {
            _addPlayerProtection = addPlayerProtection;
            _eliminatePlayer = eliminatePlayer;
            CurrentTurn = currentTurn;
            TargetPlayerId = targetPlayerId;
            GuessedCardvalue = guessedCardvalue;
            _tradeHands = tradeHands;
            _revealHand = revealHand;
            _discardAndDraw = discardAndDraw;
        }

        public Turn.Turn CurrentTurn { get; }
        public Guid? TargetPlayerId { get; }
        public CardValue? GuessedCardvalue { get; }

        public void EliminateCurrentPlayer()
        {
            Eliminate(CurrentTurn.CurrentPlayerId);
        }

        public void DiscardAndDraw(Guid targetId)
        {
            _discardAndDraw(targetId);
        }

        public void AddCurrentPlayerProtection()
        {
            _addPlayerProtection(CurrentTurn.CurrentPlayerId);
        }

        public void Eliminate(Guid targetId)
        {
            _eliminatePlayer(targetId);
        }

        public void TradeHands(Guid targetId)
        {
            _tradeHands(CurrentTurn.CurrentPlayerId, targetId);
        }

        public void RevealHand(Guid targetId)
        {
            var value = _revealHand(targetId);
            CurrentTurn.RevealHand(targetId, value);
        }

        public void GuessAndCheck(Guid targetId, CardValue guessValue)
        {
            if (guessValue == CardValue.Guard)
            {
                throw new InvalidOperationException("Can not guess Guard Value");
            }
            var value = _revealHand(targetId); //this may be the only one which should not 'reveal hand'
            if (value == guessValue)
                Eliminate(targetId);
        }

        public void CompareHands(Guid targetId)
        {
            var value = _revealHand(targetId);
            if (CardValue.Baron == value)
            {
                //we do nothing
            }
            else if (CardValue.Baron > value)
            {
                Eliminate(targetId);
            }
            else
            {
                Eliminate(CurrentTurn.CurrentPlayerId);
            }
        }
    }
}