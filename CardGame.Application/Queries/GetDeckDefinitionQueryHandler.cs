using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using MediatR;

namespace CardGame.Application.Queries; 

public class GetDeckDefinitionQueryHandler : IRequestHandler<GetDeckDefinitionQuery, IEnumerable<CardDto>> 
{
    private readonly IDeckRepository _deckRepository;

    public GetDeckDefinitionQueryHandler(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository ?? throw new ArgumentNullException(nameof(deckRepository));
    }

    public async Task<IEnumerable<CardDto>> Handle(GetDeckDefinitionQuery request, CancellationToken cancellationToken)
    {
        var domainCards = await _deckRepository.GetDeckByIdAsync(request.DeckId).ConfigureAwait(false);

        if (domainCards == null)
        {
            return Enumerable.Empty<CardDto>(); // Or throw, or return null if appropriate for downstream
        }

        var cardDtos = domainCards.Select(card => new CardDto
        {
            Rank = card.Rank,
            AppearanceId = card.AppearanceId
        }).ToList();

        return cardDtos;
    }
}
