using System.ComponentModel.DataAnnotations;
using CardGame.Application.Common.Interfaces;
using CardGame.Domain.Game;
using CardGame.Domain.Interfaces;
using MediatR;

namespace CardGame.Application.Commands;

/// <summary>
/// Handles the CreateGameCommand.
/// </summary>
public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, Guid>
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserRepository _userRepository;

    public CreateGameCommandHandler(IGameRepository gameRepository, IUserRepository userRepository)
    {
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<Guid> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        // --- Validation (FluentValidation pipeline handles basic checks) ---

        // 1. Fetch user details for all provided Player IDs to get usernames
        var playerNamesForGame = new List<string>();
        foreach(var playerId in request.PlayerIds)
        {
            // Use the new repository method to get the User model
            var user = await _userRepository.GetUserByIdAsync(playerId);
            if (user == null)
            {
                 // Player ID provided doesn't correspond to a known user
                 throw new ValidationException($"Player ID '{playerId}' not found.");
            }
            playerNamesForGame.Add(user.Username); // Add the retrieved username
        }

        // 2. Check for duplicate usernames (unlikely if IDs are unique, but good practice)
        if (playerNamesForGame.Distinct(StringComparer.OrdinalIgnoreCase).Count() != playerNamesForGame.Count)
        {
             // This indicates a potential issue with the user repository data if IDs were unique
             throw new InvalidOperationException("Duplicate usernames found for distinct player IDs.");
        }

        // 3. Creator inclusion check is handled by FluentValidation or implicitly by previous checks.

        // --- End Validation ---


        // --- Create and Save Game ---
        // Use the factory method on the Game aggregate, passing the looked-up names
        var game = Game.CreateNewGame(playerNamesForGame, request.TokensToWin ?? 4); // Use default if not specified

        // Persist the new game aggregate
        await _gameRepository.SaveAsync(game, cancellationToken);

        // Domain events raised by CreateNewGame should be published by a separate mechanism

        return game.Id;
    }
}