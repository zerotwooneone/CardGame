namespace CardGame.Domain.Turn;

public interface IShuffleService
{
    RoundCard[] Shuffle(IEnumerable<RoundCard> deck);
}