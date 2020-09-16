﻿using CardGame.Utils.Factory;

namespace CardGame.Domain.Card
{
    public class King : Card
    {
        protected King(CardId cardId) : base(cardId)
        {
        }

        protected override void OnPlayed(IPlayContext playContext)
        {
            playContext.TradeHandsWithTarget();
        }

        public override bool PreventPlay(IPlayContext playContext)
        {
            return playContext.PlayerHas(CardStrength.Countess);
        }

        public static FactoryResult<Card> Factory(int variant)
        {
            var result = CardId.Factory(CardStrength.King, variant);
            if (result.IsError)
            {
                return FactoryResult<Card>.Error(result.ErrorMessage);
            }
            return FactoryResult<Card>.Success(new King(result.Value));
        }
    }
}