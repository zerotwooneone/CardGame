using CardGame.Domain.Turn;
using Microsoft.Extensions.Logging;

namespace CardGame.Application;

public class TurnRepository : ITurnRepository
{
    private readonly ILogger<TurnRepository> _logger;
    public TurnRepository(ILogger<TurnRepository> logger)
    {
        _logger = logger;
    }
    public async Task<Turn?> GetCurrentTurn(GameId gameId)
    {
        _logger.LogWarning("returning same old game");
        var cards = Cards.AllCards;
        var shuffled =new Queue<Card>(cards.OrderBy(c => (new Random(3223)).Next()));
        var burned = shuffled.Dequeue();
        var players = new []
        {
            new Player((PlayerId)1, shuffled.Dequeue(),shuffled.Dequeue()),
            new Player((PlayerId)2, shuffled.Dequeue()),
            new Player((PlayerId)3, shuffled.Dequeue()),
            new Player((PlayerId)4, shuffled.Dequeue())
        };
        return new Turn(1,
            new Game((GameId) 1,
                players,
                cards),
            new Round(1, shuffled.ToArray(), new[] {burned}, players),
            players[0]);
    }

    public async Task Save(Turn turn)
    {
        _logger.LogError("save turn not implemented");
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
