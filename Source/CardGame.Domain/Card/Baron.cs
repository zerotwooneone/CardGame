using CardGame.Domain.Abstractions.Card;
using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class Baron : Card
    {
        protected Baron(CardId cardId) : base(cardId)
        {
        }
        protected override void CheckPreconditions(IPlayContext playContext)
        {
            playContext.HasTarget();
            playContext.TargetIsNotSelf();
        }
        protected override void OnPlayed(IPlayContext playContext)
        {
            if (playContext.PlayerHandWeaker())
            {
                playContext.EliminatePlayer();
            } else if (playContext.TargetHandWeaker())
            {
                playContext.EliminateTarget();
            }
            // dont eliminate if they match!
        }

        public static FactoryResult<Card> Factory(int variant)
        {
            var result = CardId.Factory(CardStrength.Baron, variant);
            if (result.IsError)
            {
                return FactoryResult<Card>.Error(result.ErrorMessage);
            }
            return FactoryResult<Card>.Success(new Baron(result.Value));
        }
    }
}