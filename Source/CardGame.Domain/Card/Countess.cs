using CardGame.Domain.Abstractions.Card;
using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class Countess : Card
    {
        protected Countess(CardId cardId) : base(cardId)
        {
        }
        public static FactoryResult<Card> Factory(int variant)
        {
            var result = CardId.Factory(CardStrength.Countess, variant);
            if (result.IsError)
            {
                return FactoryResult<Card>.Error(result.ErrorMessage);
            }
            return FactoryResult<Card>.Success(new Countess(result.Value));
        }
    }
}