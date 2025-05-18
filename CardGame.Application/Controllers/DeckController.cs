using CardGame.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using CardGame.Application.Queries;

namespace CardGame.Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeckController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeckController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("{deckId}")]
    public async Task<ActionResult<DeckDefinitionDto>> GetDeckDefinition(Guid deckId)
    {
        if(deckId == Guid.Empty) return BadRequest("Deck ID cannot be empty.");
        
        var query = new GetDeckDefinitionQuery(deckId);
        var result = await _mediator.Send(query).ConfigureAwait(false);

        if (result == null || result.Cards == null || !result.Cards.Any())
        {
             return NotFound("Deck not found or is empty.");
        }
        
        return Ok(result);
    }
}
