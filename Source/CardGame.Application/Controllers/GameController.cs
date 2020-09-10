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
        public async Task<Game> Get([FromQuery]string gameId)
        {
            var gameDao = await _gameDal.GetById(gameId).ConfigureAwait(false);
            return _gameConverter.ConvertGame(gameDao);
        }

        [HttpPost]
        [Route("{gameId}/Play")]
        public async Task<PlayResponse> Post(string gameId, PlayRequest request)
        {
            var response =
                await _bus.Request<PlayRequest, PlayResponse>("CardGame.Domain.Abstractions.Game.IPlayService", "Play", "CardPlayed", request.EventId, request);
            return response;
        }
    }
}