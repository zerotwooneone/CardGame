using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class Handmaid : Card
    {
        protected Handmaid(CardId cardId) : base(cardId)
        {
        }

        protected override void OnPlayed(IPlayContext playContext)
        {
            playContext.ProtectPlayer();
        }

        public static FactoryResult<Card> Factory(int variant)
        {
            var result = CardId.Factory(CardStrength.Handmaid, variant);
            if (result.IsError)
            {
                return FactoryResult<Card>.Error(result.ErrorMessage);
            }
            return FactoryResult<Card>.Success(new Handmaid(result.Value));
        }
    }
}