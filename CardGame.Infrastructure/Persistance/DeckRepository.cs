using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using CardGame.Domain.Types; 

namespace CardGame.Infrastructure.Persistance;

public class DeckRepository : IDeckRepository
{
    private static readonly Dictionary<CardType, (string MainAsset, Func<CardType, IEnumerable<CardAsset>>? assetGenerator)> _defaultDeckAssets = new()
    {
        { CardType.Guard,    ("assets/images/cards/guard.png", null) },
        { CardType.Priest,   ("assets/images/cards/priest.png", null) },
        { CardType.Baron,    ("assets/images/cards/baron.png", null) },
        { CardType.Handmaid, ("assets/images/cards/handmaid.png", null) },
        { CardType.Prince,   ("assets/images/cards/prince.png", null) },
        { CardType.King,     ("assets/images/cards/king.png", null) },
        { CardType.Countess, ("assets/images/cards/countess.png", null) },
        { CardType.Princess, ("assets/images/cards/princess.png", null) }
    };

    public Task<IEnumerable<CardAsset>> GetDeckByIdAsync(Guid deckId)
    {
        var cardAssets = CardType.List().Aggregate(new List<CardAsset>(), (list, cardType) =>
        {
            if (_defaultDeckAssets.TryGetValue(cardType, out var assetData))
            {
                if(assetData.assetGenerator != null)
                {
                    list.AddRange(assetData.assetGenerator(cardType));
                    return list;
                }
                var cardDef = new CardAsset
                {
                    Rank = cardType.Value, 
                    Asset = assetData.MainAsset
                };
                var count = cardType.Value switch
                {
                    8 => 1,
                    7 => 1,
                    6 => 1,
                    5 => 2,
                    4 => 2,
                    3 => 2,
                    2 => 2,
                    1 => 5,
                    _ => 1
                };
                list.AddRange(Enumerable.Repeat(cardDef, count));
            }
            else
            {
                throw new ArgumentException("Card type not found in static asset definitions.");
            }
             
            return list;
        }).AsEnumerable(); 
        
        return Task.FromResult(cardAssets.ToArray().AsEnumerable());
    }
}
