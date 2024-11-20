namespace CardGame.Domain.Turn;

public interface IShuffleService
{
    Card[] Shuffle(IEnumerable<Card> deck);
}