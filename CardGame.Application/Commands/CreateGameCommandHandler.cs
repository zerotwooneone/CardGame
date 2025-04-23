using System.ComponentModel.DataAnnotations;
using CardGame.Application.Common.Interfaces;
using CardGame.Domain.Common;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using MediatR;

namespace CardGame.Application.Commands;

/// <summary>
/// Handles the CreateGameCommand. Creates the game and immediately starts the first round.
/// </summary>
public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Guid>
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRandomizer _randomizer; // Inject randomizer for StartNewRound

    public CreateGameCommandHandler(
        IGameRepository gameRepository,
        IUserRepository userRepository,
        IRandomizer randomizer) // Add IRandomizer dependency
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _randomizer = randomizer ?? throw new ArgumentNullException(nameof(randomizer)); // Use injected randomizer
    }

    public async Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        // --- Validation ---
        // (Assuming FluentValidation pipeline runs first or validation is done here)

        // 1. Fetch user details (including username) for all provided Player IDs
        // Use PlayerInfo record (defined elsewhere, assumes public record PlayerInfo(Guid Id, string Name);)
        var playerInfosForGame = new List<PlayerInfo>();
        foreach (var playerId in request.PlayerIds)
        {
            var user = await _userRepository.GetUserByIdAsync(playerId);
            if (user == null)
            {
                // Player ID provided doesn't correspond to a known user
                throw new ValidationException($"Player ID '{playerId}' not found.");
            }

            // Add PlayerInfo containing both ID and Username
            playerInfosForGame.Add(new PlayerInfo(user.PlayerId, user.Username));
        }

        // 2. Check for duplicate usernames (unlikely if IDs are unique, but good practice)
        // Use the retrieved usernames from playerInfosForGame
        if (playerInfosForGame.Select(p => p.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() !=
            playerInfosForGame.Count)
        {
            throw new InvalidOperationException("Duplicate usernames found for distinct player IDs.");
        }
        // --- End Validation ---


        // --- Create Game ---
        // **ASSUMPTION:** Game.CreateNewGame needs to be refactored to accept IEnumerable<PlayerInfo>
        // instead of IEnumerable<string>.
        // The updated factory method would then use both the ID and Name from PlayerInfo
        // when creating the Player entities internally (likely calling Player.Load or similar).
        var game = Game.CreateNewGame(playerInfosForGame, request.TokensToWin ?? 4);

        // --- Start First Round ---
        // Call StartNewRound immediately after creation, using the injected randomizer
        game.StartNewRound(_randomizer);

        // --- Save Final State ---
        // Persist the game state *after* the first round has been initialized
        await _gameRepository.SaveAsync(game, cancellationToken);

        // Domain events raised by CreateNewGame AND StartNewRound should be published
        // by a separate mechanism after this handler completes successfully.

        return game.Id;
    }
}