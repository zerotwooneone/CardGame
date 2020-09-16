using CardGame.Utils.Validation;

namespace CardGame.Domain.Card
{
    public abstract class Card
    {
        public CardId CardId { get; }

        protected Card(CardId cardId)
        {
            CardId = cardId;
        }
        public override int GetHashCode()
        {
            return CardId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return CardId.Equals((obj as Card)?.CardId);
        }

        public void Play(IPlayContext playContext, Notification note)
        {
            if (PreventPlay(playContext))
            {
                note.AddError($"Cannot play {CardId}");
            }
            else
            {
                OnPlayed(playContext);
            }
        }

        //todo: change this to "must play" and implement on Countess
        public virtual bool PreventPlay(IPlayContext playContext)
        {
            return false;
        }

        protected abstract void OnPlayed(IPlayContext playContext);

        public virtual void Discard(IPlayContext playContext)
        {
            // do nothing
        }
    }
}