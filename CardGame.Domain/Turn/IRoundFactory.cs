namespace CardGame.Domain.Turn;

public interface IRoundFactory
{
    Round CreateFrom(
        uint roundNumber, 
        GamePlayer first, 
        IEnumerable<GamePlayer> playerOrder, 
        IEnumerable<Card> deck,
        IShuffleService shuffleService);
}