using CardGame.Domain.Abstractions.Card;
using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class Princess : Card
    {
        protected Princess(CardId cardId) : base(cardId)
        {
        }

        public override void Discard(IPlayContext playContext)
        {
            playContext.EliminatePlayer();
        }

        public static FactoryResult<Card> Factory(int variant)
        {
            var result = CardId.Factory(CardStrength.Princess, variant);
            if (result.IsError)
            {
                return FactoryResult<Card>.Error(result.ErrorMessage);
            }
            return FactoryResult<Card>.Success(new Princess(result.Value));
        }
    }
}