using System;
using System.Threading.Tasks;
using CardGame.Application.DTO;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Game;
using CardGame.Utils.Abstractions.Bus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
        public async Task<ActionResult> Post(string gameId, PlayRequest request)
        {
            //todo: use separate model for controller param from service param
            if (!Guid.TryParse(gameId, out var gid)) return new BadRequestResult();
            if (request.GameId != gid)
            {
                _logger.LogWarning($"Url Game Id does not match request game id url:{gameId} request:{request.GameId}");
            }

            if (request.CorrelationId == default)
            {
                request.CorrelationId = Guid.NewGuid();
            }
            request.GameId = gid;
            
            var response =
                await _bus.Request<PlayRequest, CardPlayed>("CardGame.Domain.Abstractions.Game.IPlayService:Play", request);
            return new JsonResult(response);
        }
    }
}