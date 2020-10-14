using CardGame.Domain.Abstractions.Card;

namespace CardGame.Domain.Abstractions.Game
{
    public class PlayResult
    {
        public PlayResult(ICardId revealedTargetCard)
        {
            RevealedTargetCard = revealedTargetCard;
        }

        public ICardId RevealedTargetCard { get; }
    }
}