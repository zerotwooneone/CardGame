using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame.Core.Round
{
    public class RoundContext
    {
        private readonly Action<Card.Card> _onForcedDiscard;
        private readonly Action<Guid> _addPlayerProtection;
        private readonly Func<Card.Card> _drawCard;
        private readonly Action<Guid> _eliminatePlayer;
        
        public RoundContext(Action<Card.Card> onForcedDiscard,
            Action<Guid> addPlayerProtection,
            Func<Card.Card> drawCard,
            Action<Guid> eliminatePlayer)
        {
            _onForcedDiscard = onForcedDiscard;
            _addPlayerProtection = addPlayerProtection;
            _drawCard = drawCard;
            _eliminatePlayer = eliminatePlayer;
        }

        public Turn.Turn CurrentTurn { get; private set; }
        
        public void EliminateCurrentPlayer()
        {
            var currentPlayer = CurrentTurn.CurrentPlayer;
            Eliminate(currentPlayer);
        }

        public void DiscardAndDraw(Player.Player target)
        {
            var newCard = Draw();
            var discard = target.Hand.Replace(newCard);
            _onForcedDiscard(discard);
        }

        private Card.Card Draw()
        {
            return _drawCard();
        }

        public void AddCurrentPlayerProtection()
        {
            var player = CurrentTurn.CurrentPlayer;
            _addPlayerProtection(player.Id);
        }

        public void Eliminate(Player.Player target)
        {
            var targetId = target.Id;
            _eliminatePlayer(targetId);
        }
    }
}