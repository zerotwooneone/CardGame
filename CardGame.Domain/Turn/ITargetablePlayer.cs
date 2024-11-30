namespace CardGame.Domain.Turn;

public interface ITargetablePlayer
{
    PlayerId Id { get; }
    Card DiscardAndDraw(Card card);
    Card Trade(Card otherHand);
}