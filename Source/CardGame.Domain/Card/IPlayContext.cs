using CardGame.Domain.Abstractions.Card;

namespace CardGame.Domain.Card
{
    //todo: move this to abstractions
    public interface IPlayContext
    {
        void EliminatePlayer();
        void EliminateTarget();
        bool PlayerHas(CardStrength cardStrength);
        void ProtectPlayer();
        void RevealTargetHand();
        void TargetDiscardAndDraw();
        void TradeHandsWithTarget();
        bool IsTargetOtherPlayer();
        bool GuessIsNot(ICardValue cardValue);
        bool TargetHandMatchesGuess();
        bool PlayerHandWeaker();
        bool TargetHandWeaker();
    }
}