using CardGame.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public async Task<ActionResult<IEnumerable<CardAsset>>> GetDeckDefinition(Guid deckId)
    {
        if(deckId == Guid.Empty) return BadRequest();
        var query = new GetDeckDefinitionQuery(deckId);
        var result = await _mediator.Send(query).ConfigureAwait(false);
        return Ok(result);
    }
}
