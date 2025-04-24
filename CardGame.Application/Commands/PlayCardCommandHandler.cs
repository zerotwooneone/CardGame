using CardGame.Domain.Interfaces;
using CardGame.Domain.Types;
using MediatR;

namespace CardGame.Application.Commands;

/// <summary>
/// Handles the PlayCardCommand.
/// </summary>
public class PlayCardCommandHandler : IRequestHandler<PlayCardCommand>
{
    private readonly IGameRepository _gameRepository;
    // No need for IUserRepository or IAuthService if PlayerId is passed in command

    public PlayCardCommandHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
    }

    public async Task Handle(PlayCardCommand request, CancellationToken cancellationToken)
    {
        // 1. Load Game Aggregate
        var game = await _gameRepository.GetByIdAsync(request.GameId, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            // Consider a specific exception type or handle not found appropriately
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

        // 5. Check if player holds the specific card instance they intend to play
        // Uses the explicit IsSameCard helper method for clarity
        if (!player.Hand.GetCards().Any(c => c.IsSameCard(request.CardToPlay)))
        {
            throw new InvalidOperationException($"Player {request.PlayerId} does not hold the specified card instance (ID: {request.CardToPlay.Id}) in game {request.GameId}.");
        }

        // 6. Check hand size - player must have 2 cards to be able to choose one to play
        if (player.Hand.Count < 2)
        {
            // This state shouldn't normally occur if turn logic is correct, but good fail-safe
            throw new InvalidOperationException($"Player {request.PlayerId} must have 2 cards to play, but has {player.Hand.Count} in game {request.GameId}.");
        }

        // --- End Fail Fast Validations ---


        // 7. Execute the domain logic by calling the aggregate method
        // The Game.PlayCard method contains further domain-specific validation (target validity, Countess rule, etc.)
        game.PlayCard(
            request.PlayerId,
            request.CardToPlay,
            request.TargetPlayerId,
            request.GuessedCardType
        );

        // 8. Save the updated game state
        await _gameRepository.SaveAsync(game, cancellationToken).ConfigureAwait(false);

        // 9. Publish Domain Events (handled by separate mechanism, e.g., pipeline behavior or repository decorator)

    }
}