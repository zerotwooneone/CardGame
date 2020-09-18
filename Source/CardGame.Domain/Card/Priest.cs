using CardGame.Domain.Abstractions.Card;
using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class Priest : Card
    {
        protected Priest(CardId cardId) : base(cardId)
        {
        }
        protected override void CheckPreconditions(IPlayContext playContext)
        {
            playContext.HasTarget();
            playContext.TargetIsNotSelf();
        }
        protected override void OnPlayed(IPlayContext playContext)
        {
            playContext.RevealTargetHand();
        }

        public static FactoryResult<Card> Factory(int variant)
        {
            var result = CardId.Factory(CardStrength.Priest, variant);
            if (result.IsError)
            {
                return FactoryResult<Card>.Error(result.ErrorMessage);
            }
            return FactoryResult<Card>.Success(new Priest(result.Value));
        }
    }
}