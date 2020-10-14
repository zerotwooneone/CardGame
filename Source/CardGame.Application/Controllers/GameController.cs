using System;
using System.Threading;
using System.Threading.Tasks;
using CardGame.Application.DTO;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Game;
using CardGame.Utils.Abstractions.Bus;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IHostingEnvironment _hostingEnvironment;

        public GameController(ILogger<GameController> logger,
            IGameDal gameDal,
            IGameConverter gameConverter,
            IBus bus,
            IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _gameDal = gameDal;
            _gameConverter = gameConverter;
            _bus = bus;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [Route("{gameId}")]
        public async Task<CommonKnowledgeGame> Get(Guid gameId)
        {
            var gameDao = await _gameDal.GetById(gameId).ConfigureAwait(false);
            return _gameConverter.ConvertToCommonKnowledgeGame(gameDao);
        }

        [HttpPost]
        [Route("{gameId}/Play")]
        public async Task<ActionResult> Post(Guid gameId, PlayRequest request)
        {
            var correlationId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var serviceRequest = new Domain.Abstractions.Game.PlayRequest
            {
                CorrelationId = correlationId,
                GameId = gameId,
                CardStrength = request.CardStrength,
                CardVarient = request.CardVariant,
                PlayerId = request.PlayerId,
                GuessValue = request.GuessValue,
                TargetId = request.TargetId,
                EventId = eventId,
            };

            var cancellationToken = GetRequestCancellationToken();

            var response =
                await _bus.Request<Domain.Abstractions.Game.PlayRequest, CardPlayed>(
                    "CardGame.Domain.Abstractions.Game.IGameService:Play", 
                    serviceRequest, 
                    eventId: eventId,
                    cancellationToken: cancellationToken);
            if (_hostingEnvironment.IsDevelopment())
            {
                var result = new JsonResult(response);
                return result;
            }
            if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                var result = Accepted(new {CorrelationId = correlationId});
                result.Location = GetGameUrl(gameId);
                return result;
            }
            

            return BadRequest(response.ErrorMessage);
        }

        private string GetGameUrl(Guid gameId)
        {
            return $"~/game/{gameId}";
        }

        private CancellationToken GetRequestCancellationToken()
        {
            CancellationTokenSource cts = _hostingEnvironment.IsDevelopment()
                ? new CancellationTokenSource()
                : new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var cancellationToken = cts.Token;
            return cancellationToken;
        }

        [HttpGet]
        [Route("{gameId}/Player/{playerId}")]
        public async Task<PlayerDto> GetPlayer(Guid gameId, Guid playerId)
        {
            var gameDao = await _gameDal.GetById(gameId).ConfigureAwait(false);
            return _gameConverter.ConvertToPlayer(gameDao, playerId);
        }

        [HttpPost]
        [Route("{gameId}/reset")]
        public async Task<ActionResult> Post(Guid gameId)
        {
            await ((FakeGameDal) _gameDal).Reset();
            var result = await Get(gameId);
            return Ok(result);
        }
    }
}