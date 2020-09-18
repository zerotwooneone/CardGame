using CardGame.Domain.Abstractions.Card;
using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class Guard : Card
    {
        protected Guard(CardId cardId) : base(cardId)
        {
        }
        protected override void CheckPreconditions(IPlayContext playContext)
        {
            playContext.HasTarget();
            playContext.TargetIsNotSelf();
            playContext.HasGuessValue();
            playContext.GuessIsNot(CardStrength.Guard);
        }
        protected override void OnPlayed(IPlayContext playContext)
        {
            if (playContext.TargetHandMatchesGuess())
            {
                playContext.EliminateTarget();
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