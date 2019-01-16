using System;
using CardGame.Core.Card;

namespace CardGame.Core.Round
{
    public interface IPlayRound
    {
        Guid? PlayKing(Guid cardId, Guid playerId, Guid targetId);
        void PlayPrincess(Guid cardId, Guid playerId);
        void PlayPrince(Guid cardId, Guid playerId, Guid targetId);
        void PlayHandmaid(Guid cardId, Guid playerId);
        void PlayBaron(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playCardId"></param>
        /// <param name="playerId"></param>
        /// <param name="targetId"></param>
        /// <returns>False if the effect is blocked by protection. True if the card has an effect.</returns>
        bool PlayPriest(Guid playCardId, Guid playerId, Guid targetId);
        void PlayGuard(Guid cardId, Guid playerId, Guid targetId, CardValue targetHand,CardValue guess);
    }
}