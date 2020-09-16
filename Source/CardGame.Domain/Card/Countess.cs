using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class Countess : Card
    {
        protected Countess(CardId cardId) : base(cardId)
        {
        }

        protected override void OnPlayed(IPlayContext playContext)
        {
            // do nothing
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