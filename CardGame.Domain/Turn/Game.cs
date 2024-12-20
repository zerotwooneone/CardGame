﻿namespace CardGame.Domain.Turn;

public class Game
{
    public Game(
        GameId id, 
        IEnumerable<GamePlayer> players, 
        IEnumerable<Card> deck,
        bool discardAndDrawKickEnabled = true)
    {
        Id = id;
        Players = players.ToArray();
        Deck = deck.ToArray();
        DiscardAndDrawKickEnabled = discardAndDrawKickEnabled;
    }

    public GameId Id { get; }
    public IReadOnlyCollection<GamePlayer> Players { get; }
    public IReadOnlyCollection<Card> Deck { get; }
    public bool Complete=>Players.Count ==2
        ? Players.Any(p=> p.Tokens>=7)
        : Players.Count ==3
            ? Players.Any(p=> p.Tokens>=5)
            : Players.Any(p=> p.Tokens>=4);
    
    public bool DiscardAndDrawKickEnabled { get; }
}