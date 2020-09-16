using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class Guard : Card
    {
        protected Guard(CardId cardId) : base(cardId)
        {
        }

        protected override void OnPlayed(IPlayContext playContext)
        {
            //todo: check to make sure target is not current player
            if (playContext.GuessIsNot(CardId.CardValue))
            {
                if (playContext.TargetHandMatchesGuess())
                {
                    playContext.EliminateTarget();
                }
            }
        }

        public static FactoryResult<Card> Factory(int variant)
        {
            var result = CardId.Factory(CardStrength.Guard, variant);
            if (result.IsError)
            {
                return FactoryResult<Card>.Error(result.ErrorMessage);
            }
            return FactoryResult<Card>.Success(new Guard(result.Value));
        }
    }
}