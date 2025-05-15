using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using System.Linq;

namespace CardGame.Infrastructure.Persistence;

public class DeckRepository : IDeckRepository
{
    private readonly IDeckProvider _deckProvider;

    public DeckRepository(IDeckProvider deckProvider)
    {
        _deckProvider = deckProvider ?? throw new ArgumentNullException(nameof(deckProvider));
    }

    public Task<DeckDefinitionDto> GetDeckByIdAsync(Guid deckId)
    {
        // deckId is currently unused as we only have one default deck provider.
        // In a future system with multiple decks, deckId would be used by IDeckProvider.
        var domainDeckDefinition = _deckProvider.GetDeck();

        var cardDtos = domainDeckDefinition.Cards.Select(card => new CardDto
        {
            Rank = card.Rank, 
            AppearanceId = card.AppearanceId
        }).ToList();

        var deckDefinitionDto = new DeckDefinitionDto
        {
            Cards = cardDtos,
            BackAppearanceId = domainDeckDefinition.BackAppearanceId
        };

        return Task.FromResult(deckDefinitionDto);
    }
}
