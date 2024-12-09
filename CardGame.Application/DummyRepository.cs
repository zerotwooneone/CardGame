using CardGame.Domain.Turn;
using Microsoft.Extensions.Logging;

namespace CardGame.Application;

public class DummyRepository : ITurnRepository,IRoundFactory
{
    private readonly ILogger<DummyRepository> _logger;
    private Turn _turn;

    public DummyRepository(
        ILogger<DummyRepository> logger,
        IShuffleService shuffleService)
    {
        _logger = logger;
        _logger.LogWarning("returning same old game");
        _turn = CreatTurn(shuffleService);
    }
    public Task<Turn?> GetCurrentTurn(GameId gameId)
    {
        return Task.FromResult<Turn?>(Clone(_turn));
    }

    private Turn Clone(Turn turn)
    {
        return new Turn(turn.Number, turn.Game, turn.Round, turn.CurrentPlayer);
    }

    private Turn CreatTurn(IShuffleService shuffleService)
    {
        var cards = Cards.AllCards.ToArray();
        var players = new []
        {
            new GamePlayer((PlayerId)1),
            new GamePlayer((PlayerId)2),
            new GamePlayer((PlayerId)3),
            new GamePlayer((PlayerId)4)
        };
        var round = Internal_CreateFrom(1, players[0], players, cards, shuffleService);
        var firstPlayer = round.RemainingPlayers.First();
        return new Turn(1,
            new Game((GameId) 1,
                players,
                cards),
            round,
            ToCurrentPlayer(firstPlayer, round.DrawForTurn())
            );
    }

    private static CurrentPlayer ToCurrentPlayer(RemainingPlayer firstPlayer, Card drawnCard)
    {
        return new CurrentPlayer(firstPlayer.Id,firstPlayer.Hand,drawnCard);
    }

    public Task Save(Turn turn)
    {
        _turn = turn;
        return Task.CompletedTask;
    }

    public Task<Round> CreateFrom(
        uint roundNumber,
        GamePlayer first,
        IEnumerable<GamePlayer> playerOrder,
        IEnumerable<Card> deck,
        IShuffleService shuffleService)
    {
        return Task.FromResult(Internal_CreateFrom(roundNumber, first, playerOrder.ToArray(), deck.ToArray(), shuffleService));
    }
    private Round Internal_CreateFrom(
        uint roundNumber, 
        GamePlayer first, 
        GamePlayer[] playerOrder, 
        Card[] deck,
        IShuffleService shuffleService)
    {
        if (!playerOrder.Contains(first))
        {
            throw new Exception("first player must be in player order");
        }
        var shuffledDeck = shuffleService.Shuffle(deck);
        var burnCount = playerOrder.Length ==2
            ? 4
            : 1;
        var burnPile = shuffledDeck.Take(burnCount).ToArray();
        var drawPile = new Queue<Card>(shuffledDeck.Skip(burnCount));
            
        const int sanityMax = 5;
        var sanityCount = 0;
        var orderedPlayers = new Queue<GamePlayer>(playerOrder);
        while (!orderedPlayers.Peek().Id.Equals(first.Id) && sanityCount < sanityMax)
        {
            sanityCount++;
            orderedPlayers.Enqueue(orderedPlayers.Dequeue());
        }

        var players = orderedPlayers.Select(gp => new RemainingPlayer(
            gp.Id,
            drawPile.Dequeue(),
            true)).ToArray();
        var round = new Round(roundNumber, drawPile, burnPile, players);
        return round;
    }
}

internal static class CardValues
{
    public static readonly CardValue Guard = (CardValue) 1;
    public static readonly CardValue Priest = (CardValue) 2;
    public static readonly CardValue Baron = (CardValue) 3;
    public static readonly CardValue Handmaid = (CardValue) 4;
    public static readonly CardValue Prince = (CardValue) 5;
    public static readonly CardValue King = (CardValue) 6;
    public static readonly CardValue Countess = (CardValue) 7;
    public static readonly CardValue Princess = (CardValue) 8;
}

internal static class Cards
{
    private static List<Card> _allCards = new();
    public static IReadOnlyCollection<Card> AllCards => _allCards;
    public static IReadOnlyCollection<Card> Guards = Add(1, CardValues.Guard, 5);
    public static IReadOnlyCollection<Card> Priests = Add(6, CardValues.Priest, 2);
    public static IReadOnlyCollection<Card> Barons = Add(8, CardValues.Baron, 2);
    public static IReadOnlyCollection<Card> Handmaids = Add(10, CardValues.Handmaid, 2);
    public static IReadOnlyCollection<Card> Princes = Add(12, CardValues.Prince, 2);
    public static Card King = Add(14, CardValues.King, 1).First();
    public static Card Countess = Add(15, CardValues.Countess, 1).First();
    public static Card Princess = Add(16, CardValues.Princess, 1).First();

    private static Card[] Add(int startId, CardValue cardValue, int count)
    {
        var cards = new Card[count];
        for (uint i = 0; i < count; i++)
        {
            cards[i] = new Card{Id = (CardId)(startId + i), Value = cardValue};
        }
        _allCards.AddRange(cards);
        return cards;
    }
}
