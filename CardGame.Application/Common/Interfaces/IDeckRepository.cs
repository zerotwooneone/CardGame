using CardGame.Application.DTOs;
using CardGame.Domain.Game;

namespace CardGame.Application.Common.Interfaces;

public interface IDeckRepository
{
    Task<DeckDefinitionDto> GetDeckByIdAsync(Guid deckId);
}
