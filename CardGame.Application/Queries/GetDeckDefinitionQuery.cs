using CardGame.Application.DTOs;
using MediatR;

namespace CardGame.Application.Queries; 

public class GetDeckDefinitionQuery : IRequest<IEnumerable<CardAsset>>
{
    public Guid DeckId { get; }

    public GetDeckDefinitionQuery(Guid deckId)
    {
        DeckId = deckId;
    }
}
