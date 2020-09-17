using System.Linq;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Game;
using CardGame.Domain.Card;
using CardGame.Domain.Player;
using CardGame.Utils.Abstractions.Bus;
using CardGame.Utils.Factory;
using CardGame.Utils.Validation;

namespace CardGame.Domain.Game
{
    public class GameService: IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IBus _bus;

        public GameService(IGameRepository gameRepository,
            IBus bus)
        {
            _gameRepository = gameRepository;
            _bus = bus;
        }

        public async Task Play(PlayRequest request)
        {
            var note = new Notification();

            //todo: reduce duplication and figure out where this validation lives
            var gid = GameId.Factory(request.GameId);
            var cardId = CardId.Factory(request.CardStrength,request.CardVarient);
            var playerId = PlayerId.Factory(request.PlayerId);
            var targetId = request.TargetId.HasValue
                ? PlayerId.Factory(request.TargetId.Value)
                : null;
            var guessValue = request.GuessValue.HasValue
                ? CardValue.Factory(request.GuessValue.Value)
                : null;
            var results = new FactoryResult[] {gid, cardId, playerId, targetId, guessValue}
                .Where(r => r != null)
                .ToArray();
            var errors = results
                .Where(r => r.IsError)
                .ToArray();

            foreach (var error in errors)
            {
                note.AddError(error.ErrorMessage);
            }

            var game = await _gameRepository.GetById(gid.Value);

            if (game is null)
            {
                note.AddError($"Game not found {request.GameId}");
            }

            if (!note.HasErrors())
            {
                game.Play(playerId.Value, 
                    cardId.Value, 
                    targetId?.Value, 
                    guessValue?.Value,
                    note);
            }

            if (!note.HasErrors())
            {
                await _gameRepository.SetById(game);
            }

            _bus.PublishEvent("CardPlayed", new CardPlayed
            {
                CorrelationId = request.CorrelationId,
                ErrorMessage = note.ErrorMessage()
            });
        }
    }
}
