using System;

namespace CardGame.Core.Round
{
    public interface IPlayRound
    {
        void Play(Guid playerId, Guid playCard);
        void EliminatePlayer(Guid playerId);
        Guid? TradeHands(Guid sourcePlayerId, Guid targetPlayerId);
        void DiscardAndDraw(Guid targetId);
        void AddPlayerProtection(Guid playerId);
    }
}