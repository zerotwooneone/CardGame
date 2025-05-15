using CardGame.Application.Common.Interfaces;
using CardGame.Application.DTOs;
using MediatR;
using System.Linq; // Keep for potential future use, though not strictly needed now

namespace CardGame.Application.Queries; 

public class GetDeckDefinitionQueryHandler : IRequestHandler<GetDeckDefinitionQuery, DeckDefinitionDto> 
{
    private readonly IDeckRepository _deckRepository;

    public GetDeckDefinitionQueryHandler(IDeckRepository deckRepository)
    {
        _deckRepository = deckRepository ?? throw new ArgumentNullException(nameof(deckRepository));
    }

    public async Task<DeckDefinitionDto> Handle(GetDeckDefinitionQuery request, CancellationToken cancellationToken)
    {
        var deckDefinitionDto = await _deckRepository.GetDeckByIdAsync(request.DeckId).ConfigureAwait(false);

        // The DeckRepository now returns DeckDefinitionDto directly.
        // If deckDefinitionDto can be null, a null check might be desired here.
        // For example: if (deckDefinitionDto == null) { return null; /* or throw */ }

        return deckDefinitionDto;
    }
}
