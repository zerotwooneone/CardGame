namespace CardGame.Domain.Turn;

public interface IRoundFactory
{
    Round CreateFrom(uint roundNumber, Player first, IReadOnlyCollection<Player> playerOrder, IEnumerable<Card> deck);
}