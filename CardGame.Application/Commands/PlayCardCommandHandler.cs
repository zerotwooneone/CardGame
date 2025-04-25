using CardGame.Domain.Game.GameException;
using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using FluentValidation;
using MediatR;

namespace CardGame.Application.Commands;

    /// <summary>
    /// Handles the PlayCardCommand.
    /// </summary>
    public class PlayCardCommandHandler : IRequestHandler<PlayCardCommand>
    {
        private readonly IGameRepository _gameRepository;

        public PlayCardCommandHandler(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        }

        public async Task Handle(PlayCardCommand request, CancellationToken cancellationToken)
        {
            // 1. Load Game Aggregate
            var game = await _gameRepository.GetByIdAsync(request.GameId).ConfigureAwait(false);
            if (game == null)
            {
                throw new KeyNotFoundException($"Game with ID {request.GameId} not found.");
            }

            // --- Fail Fast Validations ---

            // 2. Check Game Phase
            if (game.GamePhase != GamePhase.RoundInProgress)
            {
                throw new InvalidOperationException($"Cannot play card. Game {request.GameId} is not in progress (Phase: {game.GamePhase.Name}).");
            }

            // 3. Check if it's the requesting player's turn
            if (game.CurrentTurnPlayerId != request.PlayerId)
            {
                throw new InvalidOperationException($"It is not player {request.PlayerId}'s turn in game {request.GameId}.");
            }

            // 4. Check if player exists and get player object
            var player = game.Players.FirstOrDefault(p => p.Id == request.PlayerId);
            if (player == null)
            {
                 throw new InvalidOperationException($"Player {request.PlayerId} not found in game {request.GameId}.");
            }

            // 5. Find the specific card instance in the player's hand using the ID
            var cardToPlayInstance = player.Hand.GetCards().FirstOrDefault(c => c.Id == request.CardToPlayId);
            if (cardToPlayInstance == null)
            {
                // Throw specific exception if card ID not found in hand
                throw new CardNotFoundInHandException($"Card with ID {request.CardToPlayId} not found in player {request.PlayerId}'s hand.");
            }

            // 6. Check hand size - player must have 2 cards to be able to choose one to play
            if (player.Hand.Count < 2)
            {
                 throw new InvalidOperationException($"Player {request.PlayerId} must have 2 cards to play, but has {player.Hand.Count} in game {request.GameId}.");
            }

            // 7. Perform validation that depends on the actual card type being played
             if (cardToPlayInstance.Type == CardType.Guard)
             {
                 if (request.GuessedCardType == null)
                     throw new ValidationException("A card type must be guessed when playing a Guard.");
                 if (request.GuessedCardType == CardType.Guard)
                     throw new ValidationException("Cannot guess Guard when playing a Guard.");
                 if (request.TargetPlayerId == null)
                      throw new ValidationException("Target player must be specified when playing a Guard.");
             }
             // Add similar checks for other cards requiring targets if not covered by Game.ValidatePlayCardAction

            // --- End Fail Fast Validations ---


            // 8. Execute the domain logic by calling the aggregate method
            // Pass the actual Card instance found in the player's hand
            game.PlayCard(
                request.PlayerId,
                cardToPlayInstance, // Pass the found instance
                request.TargetPlayerId,
                request.GuessedCardType
            );

            // 9. Save the updated game state
            await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

            // 10. Publish Domain Events (handled by separate mechanism)

            return; 
        }
    }