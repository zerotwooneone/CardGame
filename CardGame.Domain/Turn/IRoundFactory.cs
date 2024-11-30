namespace CardGame.Domain.Turn;

public interface IRoundFactory
{
    Task<Round> CreateFrom(
        uint roundNumber, 
        GamePlayer first, 
        IEnumerable<GamePlayer> playerOrder, 
        IEnumerable<Card> deck,
        IShuffleService shuffleService);
}