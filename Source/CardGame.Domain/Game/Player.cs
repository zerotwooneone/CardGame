using System;
using CardGame.Utils.Entity;
using CardGame.Utils.Factory;

namespace CardGame.Domain.Game
{
    public class Player : Entity<PlayerId>
    {
        public Hand Hand { get; }
        public Score Score { get; }

        protected Player(PlayerId id, Hand hand, Score score) : base(id)
        {
            Hand = hand;
            Score = score;
        }

        public static FactoryResult<Player> Factory(Guid id, Hand hand = null, Score score = null)
        {
            var idResult = PlayerId.Factory(id);
            if (idResult.IsError)
            {
                return FactoryResult<Player>.Error("id invalid");
            }

            hand = hand ?? Hand.Factory().Value;
            score = score ?? Score.Factory().Value;
            return FactoryResult<Player>.Success(new Player(idResult.Value, hand, score));
        }
    }
}