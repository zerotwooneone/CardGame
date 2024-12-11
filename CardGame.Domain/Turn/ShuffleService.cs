using Microsoft.Extensions.Logging;

namespace CardGame.Domain.Turn;

public class ShuffleService : IShuffleService
{
    private readonly ILogger<ShuffleService> _logger;
    private readonly Random _random;

    public ShuffleService(ILogger<ShuffleService> logger)
    {
        _logger = logger;
        
        const int nonRandomSeed = 3113;
        _logger.LogWarning("Shuffling with seed {NonRandomSeed}", nonRandomSeed);
        _random = new Random(nonRandomSeed);
    }
    public RoundCard[] Shuffle(IEnumerable<RoundCard> deck)
    {
        return deck.OrderBy(_ => _random.Next()).ToArray();
    }
}