namespace CardGame.Domain.Turn;

public record Round
{
    private readonly List<RemainingPlayer> _remainingPlayers;
    public IReadOnlyCollection<RemainingPlayer> RemainingPlayers => _remainingPlayers;
    public IReadOnlyCollection<RoundPlayer> Eliminated => _eliminated;
    private readonly List<RoundPlayer> _eliminated;
    private readonly List<Card> _drawPile;
    public IReadOnlyCollection<Card> DrawPile => _drawPile;
    private readonly List<Card> _burnPile;
    public IReadOnlyCollection<Card> BurnPile => _burnPile;
    public bool Complete => DrawPile.Count == 0 || RemainingPlayers.Count == 1;
    public uint Number { get; }

    public Round(
        uint number,
        IEnumerable<Card> drawPile,
        IEnumerable<Card> burnPile,
        IEnumerable<RemainingPlayer> remainingPlayers,
        IEnumerable<RoundPlayer>? eliminated=null)
    {
        Number = number;
        _drawPile = drawPile.ToList();
        _burnPile = burnPile.ToList();
        _remainingPlayers = [..remainingPlayers];
        _eliminated = eliminated?.ToList() ?? [];
    }

    public RemainingPlayer GetWinner()
    {
        if(!Complete)
        {
            throw new Exception("round is not complete");
        }
        if(RemainingPlayers.Count == 1)
        {
            return RemainingPlayers.First();
        }

        var maxValue = (CardValue) RemainingPlayers.Max(p => p.Hand.Value.Value);
        var playerWithMaxValue = RemainingPlayers.Where(p => p.Hand.Value.Value == maxValue.Value).ToArray();
        if(playerWithMaxValue.Length == 1)
        {
            return playerWithMaxValue[0];
        }
        var cardValueTotals = RemainingPlayers.Select(p=> new {Player = p, Total = p.DiscardPile.Sum(c => c.Value.Value)}).ToArray();
        var maxTotal = cardValueTotals.Max(p => p.Total);
        var playerWithMaxTotal = cardValueTotals.Where(p => p.Total == maxTotal).ToArray().First();
        return playerWithMaxTotal.Player;
    }

    public void EliminatePlayer(RemainingPlayer player)
    {
        _remainingPlayers.Remove(player);
        _eliminated.Add(player.ToEliminated());
    }

    public Card DrawForDiscard()
    {
        return Draw() ?? _burnPile.First();
    }

    private Card? Draw()
    {
        var result = _drawPile.FirstOrDefault();
        if(result != null)
        {
            _drawPile.Remove(result);
        }
        return result;
    }

    public Card DrawForTurn()
    {
        var result = _drawPile.First();
        _drawPile.Remove(result);
        return result;
    }
}