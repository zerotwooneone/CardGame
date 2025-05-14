using CardGame.Application.Common.Interfaces;
using CardGame.Domain.Game; 
using CardGame.Domain.Interfaces; 

namespace CardGame.Infrastructure.Persistence;

public class DeckRepository : IDeckRepository
{
    private readonly IDeckProvider _deckProvider;

    public DeckRepository(IDeckProvider deckProvider)
    {
        _deckProvider = deckProvider ?? throw new ArgumentNullException(nameof(deckProvider));
    }

    public Task<IEnumerable<Card>> GetDeckByIdAsync(Guid deckId) 
    {
        var gameDeck = _deckProvider.GetDeck();
        return Task.FromResult(gameDeck);
    }
}
