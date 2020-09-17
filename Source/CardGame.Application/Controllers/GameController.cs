using System;
using System.Threading.Tasks;
using CardGame.Application.DTO;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Game;
using CardGame.Utils.Abstractions.Bus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlayRequest = CardGame.Application.DTO.PlayRequest;

namespace CardGame.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameDal _gameDal;
        private readonly IGameConverter _gameConverter;
        private readonly IBus _bus;

        public GameController(ILogger<GameController> logger,
            IGameDal gameDal,
            IGameConverter gameConverter,
            IBus bus)
        {
            _logger = logger;
            _gameDal = gameDal;
            _gameConverter = gameConverter;
            _bus = bus;
        }

        [HttpGet]
        public async Task<CommonKnowledgeGame> Get([FromQuery]string gameId)
        {
            var gameDao = await _gameDal.GetById(gameId).ConfigureAwait(false);
            return _gameConverter.ConvertToCommonKnowledgeGame(gameDao);
        }

        [HttpPost]
        [Route("{gameId}/Play")]
        public async Task<ActionResult> Post(Guid gameId, PlayRequest request)
        {
            var serviceRequest = new Domain.Abstractions.Game.PlayRequest
            {
                CorrelationId = Guid.NewGuid(),
                GameId = gameId,
                CardStrength = request.CardStrength,
                CardVarient = request.CardVarient,
                PlayerId = request.PlayerId,
                GuessValue = request.GuessValue,
                TargetId = request.TargetId
            };
            
            var response =
                await _bus.Request<Domain.Abstractions.Game.PlayRequest, CardPlayed>("CardGame.Domain.Abstractions.Game.IGameService:Play", serviceRequest);
            if (string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                return new JsonResult(response);
            }

            return BadRequest(response.ErrorMessage);
        }
    }
}