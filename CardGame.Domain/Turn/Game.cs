namespace CardGame.Domain.Turn;

public class Game
{
    public Game(GameId id, IEnumerable<Player> players, IEnumerable<Card> deck)
    {
        Id = id;
        Players = players.ToArray();
        Deck = deck.ToArray();
    }

    public GameId Id { get; }
    public IReadOnlyCollection<Player> Players { get; }
    public IReadOnlyCollection<Card> Deck { get; }
    public bool Complete=>Players.Count ==2
        ? Players.Any(p=> p.Tokens>=7)
        : Players.Count ==3
            ? Players.Any(p=> p.Tokens>=5)
            : Players.Count ==4 && Players.Any(p=> p.Tokens>=4);
}