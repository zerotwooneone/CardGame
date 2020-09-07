using System;
using System.Threading.Tasks;
using CardGame.Application.DTO;
using CardGame.CommonModel.CommonState;
using CardGame.Domain.Abstractions.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CardGame.Application.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IGameRepository _gameRepository;
        private readonly IGameConverter _gameConverter;
        private readonly ICommonStateModelFactory _commonStateModelFactory;

        public GameController(ILogger<GameController> logger,
            IGameRepository gameRepository,
            IGameConverter gameConverter)
        {
            _logger = logger;
            _gameRepository = gameRepository;
            _gameConverter = gameConverter;
        }

        [HttpGet]
        public async Task<Game> Get([FromQuery]string gameId)
        {
            var gameDao = await _gameRepository.GetById(gameId).ConfigureAwait(false);
            return _gameConverter.ConvertGame(gameDao);
        }
    }
}