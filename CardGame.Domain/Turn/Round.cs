namespace CardGame.Domain.Turn;

public record Round
{
    private readonly List<Player> _remainingPlayers;
    public IReadOnlyCollection<Player> RemainingPlayers => _remainingPlayers;
    public IReadOnlyCollection<Player> Eliminated => _eliminated;
    private readonly List<Player> _eliminated;
    public Player CurrentPlayer => RemainingPlayers.First();
    private List<Card> _drawPile { get; }
    public IReadOnlyCollection<Card> DrawPile => _drawPile;
    private List<Card> _burnPile { get; }
    public IReadOnlyCollection<Card> BurnPile => _burnPile;
    public bool Complete => DrawPile.Count == 0 || RemainingPlayers.Count == 1;
    public uint Number { get; init; }

    public Round(
        uint number,
        IEnumerable<Card> drawPile,
        IEnumerable<Card> burnPile,
        IEnumerable<Player> remainingPlayers,
        IEnumerable<Player>? eliminated=null)
    {
        Number = number;
        _drawPile = drawPile.ToList();
        _burnPile = burnPile.ToList();
        _remainingPlayers = new List<Player>(remainingPlayers);
        _eliminated = eliminated?.ToList() ?? [];
    }

    public Player GetWinner()
    {
        if(!Complete)
        {
            throw new Exception("round is not complete");
        }
        if(RemainingPlayers.Count == 1)
        {
            return RemainingPlayers.First();
        }

        var maxValue = (CardValue) RemainingPlayers.Max(p => p.GetHand().Single().Value.Value);
        var playerWithMaxValue = RemainingPlayers.Where(p => p.GetHand().Single().Value.Value == maxValue.Value).ToArray();
        if(playerWithMaxValue.Length == 1)
        {
            return playerWithMaxValue[0];
        }
        var cardValueTotals = RemainingPlayers.Select(p=> new {Player = p, Total = p.DiscardPile.Sum(c => c.Value.Value)}).ToArray();
        var maxTotal = cardValueTotals.Max(p => p.Total);
        var playerWithMaxTotal = cardValueTotals.Where(p => p.Total == maxTotal).ToArray().First();
        return playerWithMaxTotal.Player;
    }

    public void Play(CardEffect cardEffect, Player player)
    {
        if (Complete)
        {
            throw new Exception("round is already complete");
        }
        if (player != CurrentPlayer)
        {
            throw new Exception($"not players turn {player.Id}");
        }
        if (cardEffect.KickOutOfRoundOnDiscard)
        {
            RemovePlayer(CurrentPlayer);
        }
    }

    public void RemovePlayer(Player player)
    {
        _remainingPlayers.Remove(player);
        _eliminated.Add(player);
    }

    public Player GetTargetPlayer(CardEffect cardEffect, PlayParams playParams)
    {
        if (!cardEffect.RequiresTargetPlayer)
        {
            throw new Exception("no target required");
        }
        if (playParams.TargetPlayer == null)
        {
            throw new Exception("missing target player");
        }

        var possibleTargets = cardEffect.CanTargetSelf
            ? RemainingPlayers
            : RemainingPlayers.Where(p => !p.Equals(CurrentPlayer));
        var selectedTarget = possibleTargets.FirstOrDefault(p => playParams.TargetPlayer.HasValue && p.Id == playParams.TargetPlayer.Value);
        if (selectedTarget == null)
        {
            throw new Exception($"invalid target player {playParams.TargetPlayer}");
        }

        return selectedTarget;
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

    public void DiscardAndDraw(CardEffect cardEffect, Player player)
    {
        if (cardEffect.DiscardAndDrawKickEnabled && cardEffect.KickOutOfRoundOnDiscard)
        {
            RemovePlayer(player);
        }
    }

    public Card DrawForTurn()
    {
        var result = _drawPile.First();
        _drawPile.Remove(result);
        return result;
    }

    public void NextPlayer()
    {
        var current = CurrentPlayer;
        _remainingPlayers.Remove(current);
        _remainingPlayers.Add(current);
    }
}