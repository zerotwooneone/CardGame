namespace CardGame.Domain.Turn;

public interface ITargetablePlayer
{
    PlayerId Id { get; }
    bool IsProtected { get; }
    Card DiscardAndDraw(Card card);
    Card Trade(Card otherHand);
}