using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using MediatR;

namespace CardGame.Application.Queries; 

public class GetDeckDefinitionQueryHandler : IRequestHandler<GetDeckDefinitionQuery, IEnumerable<CardAsset>>
{
    private readonly IDeckRepository _deckRepository;

    public GetDeckDefinitionQueryHandler(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository ?? throw new ArgumentNullException(nameof(deckRepository));
    }

    public async Task<IEnumerable<CardAsset>> Handle(GetDeckDefinitionQuery request, CancellationToken cancellationToken)
    {
        return await _deckRepository.GetDeckByIdAsync(request.DeckId).ConfigureAwait(false);
    }
}
