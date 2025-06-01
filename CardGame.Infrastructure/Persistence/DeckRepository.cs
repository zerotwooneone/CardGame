using CardGame.Application.Common.Interfaces; // For IDeckRepository
using CardGame.Application.DTOs;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces; // For IDeckRegistry
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardGame.Infrastructure.Persistence;

public class DeckRepository : IDeckRepository
{
    private readonly IDeckRegistry _deckRegistry;

    public DeckRepository(IDeckRegistry deckRegistry)
    {
        _deckRegistry = deckRegistry ?? throw new ArgumentNullException(nameof(deckRegistry));
    }

    public Task<DeckDefinitionDto> GetDeckByIdAsync(Guid deckId)
    {
        if (deckId == Guid.Empty)
        {
            throw new ArgumentException("DeckId cannot be an empty GUID.", nameof(deckId));
        }

        var provider = _deckRegistry.GetProvider(deckId);
        if (provider == null)
        {
            throw new KeyNotFoundException($"No deck provider found for DeckId: {deckId}");
        }

        var domainDeckDefinition = provider.GetDeck();

        var cardDtos = domainDeckDefinition.Cards.Select(card => new CardDto
        {
            Rank = card.Rank.Value,
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
