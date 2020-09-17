using System;
using CardGame.Domain.Abstractions.Card;
using CardGame.Domain.Card;
using CardGame.Domain.Game;
using CardGame.Utils.Entity;
using CardGame.Utils.Factory;
using CardGame.Utils.Validation;

namespace CardGame.Domain.Player
{
    public class Player : Entity<PlayerId>
    {
        public Hand Hand { get; protected set; }
        public Score Score { get; protected set; }
        public ProtectState Protected { get; protected set; }

        protected Player(PlayerId id, Hand hand, Score score, ProtectState protectState) : base(id)
        {
            Hand = hand;
            Score = score;
            Protected = protectState;
        }

        public static FactoryResult<Player> Factory(Guid id, Hand hand = null, Score score = null, ProtectState protectState = null)
        {
            var idResult = PlayerId.Factory(id);
            if (idResult.IsError)
            {
                return FactoryResult<Player>.Error("id invalid");
            }

            hand = hand ?? Hand.Factory().Value;
            score = score ?? Score.Factory().Value;
            var prot = protectState ?? ProtectState.UnProtected;
            return FactoryResult<Player>.Success(new Player(idResult.Value, hand, score, prot));
        }

        public void Discard(ICardId cardId, Notification note)
        {
            var newHand = Hand.Discard(cardId, note);
            if (Hand.Equals(newHand))
            {
                return;
            }

            note.AddStateChange(nameof(Player));
            Hand = newHand;
        }

        public void Replace(CardId drawn, Notification note)
        {
            Hand = Hand.Replace(drawn, note);
        }

        public void Protect(Notification note)
        {
            Protected = ProtectState.Protected;
            note.AddStateChange(nameof(Player));
        }

        public bool HandIsWeaker(Player target, Notification note)
        {
            if (target == null)
            {
                note.AddError("target player cannot be null");
                return false;
            }
            return Hand.IsWeaker(target.Hand, note);
        }

        public void ClearProtection(Notification note)
        {
            var oldValue = Protected;
            Protected = ProtectState.UnProtected;
            if (!oldValue.Equals(Protected))
            {
                note.AddStateChange(nameof(Player));
            }
        }
    }
}