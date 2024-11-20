namespace CardGame.Domain.Turn;

public class RoundFactory : IRoundFactory
{
    private readonly IShuffleService _shuffleService;

    public RoundFactory(IShuffleService shuffleService)
    {
        _shuffleService = shuffleService;
    }
    public Round CreateFrom(uint roundNumber, Player first, IReadOnlyCollection<Player> playerOrder, IEnumerable<Card> deck)
    {
        if (!playerOrder.Contains(first))
        {
            playerOrder= new[] {first}.Concat(playerOrder).ToArray();
        }
        var shuffledDeck = _shuffleService.Shuffle(deck);
        var burnCount = playerOrder.Count ==2
            ? 4
            : 1;
        var burnPile = shuffledDeck.Take(burnCount).ToArray();
        var drawPile = shuffledDeck.Skip(burnCount).ToArray();
        
        int sanityMax = 5;
        int sanityCount = 0;
        var players = new Queue<Player>(playerOrder);
        while (!players.Peek().Equals(first) && sanityCount < sanityMax)
        {
            sanityCount++;
            players.Enqueue(players.Dequeue());
        }

        var round = new Round(roundNumber, drawPile, burnPile, players);
        foreach (var player in players)
        {
            var drawn = round.DrawForTurn();
            player.StartTurn(drawn);
        }
        return round;
    }
}