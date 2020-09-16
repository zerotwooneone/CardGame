using CardGame.Utils.Validation;

namespace CardGame.Domain.Card
{
    public class PlayContext : IPlayContext
    {
        private readonly Player.Player _player;
        private readonly Player.Player _target;
        private readonly CardValue _guessValue;
        private readonly Game.Game _game;
        private readonly Notification _note;
        private readonly Card _targetCard;
        public bool DoRevealTargetHand { get; protected set; }

        public PlayContext(Player.Player player,
            Player.Player target,
            CardValue guessValue,
            Game.Game game,
            Card targetCard,
            Notification note)
        {
            _player = player;
            _target = target;
            _guessValue = guessValue;
            _game = game;
            _note = note;
            _targetCard = targetCard;
        }

        public void EliminateTarget()
        {
            _game.EliminateFromRound(_target.Id, _note);
        }
        public void EliminatePlayer()
        {
            _game.EliminateFromRound(_player.Id, _note);
        }

        public bool PlayerHas(CardStrength cardStrength)
        {
            return _player.Hand.HasCard(cardStrength);
        }

        public void ProtectPlayer()
        {
            _player.Protect(_note);
        }

        public void RevealTargetHand()
        {
            DoRevealTargetHand = true;
        }

        public void TargetDiscardAndDraw()
        {
            _targetCard.Discard(this);
            
            var drawn = _game.Draw(_note);
            _target.Replace(drawn, _note);
            _game.Discard(_targetCard.CardId, _note);
        }

        public void TradeHandsWithTarget()
        {
            var targetCard = _target.Hand.Card1;
            _target.Replace(_player.Hand.Card1, _note);
            _player.Replace(targetCard, _note);
        }

        public bool IsTargetOtherPlayer()
        {
            return !_player.Id.Equals(_target.Id);
        }

        public bool GuessIsNot(CardValue cardValue)
        {
            return !_guessValue.Equals(cardValue);
        }

        public bool TargetHandMatchesGuess()
        {
            return _target.Hand.Card1.CardValue.Value == _guessValue.Value;
        }

        public bool PlayerHandWeaker()
        {
            if (_player == null)
            {
                _note.AddError("player cannot be null");
                return false;
            }
            return _player.HandIsWeaker(_target, _note);
        }

        public bool TargetHandWeaker()
        {
            if (_target == null)
            {
                _note.AddError("target cannot be null");
                return false;
            }
            return _target.HandIsWeaker(_player, _note);;
        }
    }
}