using System;
using System.Threading.Tasks;
using CardGame.CommonModel.Bus;
using CardGame.Domain.Abstractions.Game;
using CardGame.Domain.Card;
using CardGame.Domain.Player;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Domain.Game
{
    public class GameService: IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IBus _bus;
        private readonly ValidationEngine _validationEngine;

        public GameService(IGameRepository gameRepository,
            IBus bus)
        {
            _gameRepository = gameRepository;
            _bus = bus;
            
            //todo: interface and inject
            _validationEngine = new ValidationEngine();
        }

        public async Task Play(PlayRequest request)
        {
            var gameId = _validationEngine.GetGameId(request.GameId);
            var playerId = _validationEngine.GetPlayerId(request.PlayerId);
            var targetId = _validationEngine.GetPlayerId(request.TargetId);
            var cardId = _validationEngine.GetCardId(request.CardStrength, request.CardVarient);
            var guessValue = _validationEngine.GetCardValue(request.GuessValue);
            
            var game = await _gameRepository.GetById(gameId);

            var notification = game.Play(playerId, cardId, targetId, guessValue);

            _bus.PublishEvent("CardPlayed", new CardPlayed
            {
                CorrelationId = request.CorrelationId,
            });
        }
    }

    public class ValidationEngine
    {
        public GameId GetGameId(Guid id)
        {
            var result = GameId.Factory(id);
            if (result.IsError)
            {
                throw new Exception(result.ErrorMessage);
            }

            return result.Value;
        }

        public PlayerId GetPlayerId(Guid id)
        {
            var result = PlayerId.Factory(id);
            if (result.IsError)
            {
                throw new Exception(result.ErrorMessage);
            }

            return result.Value;
        }

        public CardId GetCardId(int cardStrength, int cardVarient)
        {
            var valueResult = CardValue.Factory(cardStrength);
            if (valueResult.IsError)
            {
                throw new Exception(valueResult.ErrorMessage);
            }

            var result = CardId.Factory(valueResult.Value, cardVarient);
            if (result.IsError)
            {
                throw new Exception(result.ErrorMessage);
            }

            return result.Value;
        }

        public CardValue GetCardValue(int guessValue)
        {
            var result = CardValue.Factory(guessValue);
            if (result.IsError)
            {
                throw new Exception($"invalid card value {guessValue}");
            }

            return result.Value;
        }
    }
}
