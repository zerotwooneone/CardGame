using CardGame.Domain.Common;
using CardGame.Domain.Game;
using CardGame.Domain.Types;
using Microsoft.Extensions.Logging;

namespace CardGame.Infrastructure;

internal class DeterministicRandomizer: IRandomizer
{
    private readonly ILogger<DeterministicRandomizer> _logger;
    public DeterministicRandomizer(ILogger<DeterministicRandomizer> logger)
    {
        _logger = logger;
        _logger.LogWarning("Using DeterministicRandomizer");
    }
    public void Shuffle<T>(IList<T> list)
    {
        _logger.LogWarning("Using DeterministicRandomizer to shuffle");
        var casted = (List<Card>) list;
        var guards = casted.Where(c=>c.Type == CardType.Guard).Take(3).ToList();
        var priestCard = casted.First(c=>c.Type == CardType.Priest);
        var princess= casted.First(c=>c.Type == CardType.Princess);
        casted.Clear();
        casted.Add(princess);
        casted.Add(priestCard);
        casted.AddRange(guards);
        
    }
}