using CardGame.Domain.Abstractions.Card;

namespace CardGame.Domain.Card
{
    //todo: move this to abstractions
    public interface IPlayContext
    {
        void EliminateTarget();
        bool PlayerHas(CardStrength cardStrength);
        void ProtectPlayer();
        void RevealTargetHand();
        void TargetDiscardAndDraw();
        void TradeHandsWithTarget();
        bool TargetHandMatchesGuess();
        bool PlayerHandWeaker();
        bool TargetHandWeaker();

        //todo: create IDiscardContext
        void EliminatePlayer();

        //todo: create IPreconditionContext
        void GuessIsNot(CardStrength cardValue);
        void HasTarget();
        void TargetIsNotSelf();
        void HasGuessValue();
    }
}