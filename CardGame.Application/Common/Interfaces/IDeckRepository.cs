using CardGame.Domain.Game;

namespace CardGame.Application.Common.Interfaces;

public interface IDeckRepository
{
    Task<IEnumerable<Card>> GetDeckByIdAsync(Guid deckId);
}
