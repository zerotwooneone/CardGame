using CardGame.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace CardGame.Application.Commands;

/// <summary>
/// Command to create a new game.
/// </summary>
/// <param name="PlayerIds">Player IDs of players joining (2-4).</param> // Changed parameter name
/// <param name="CreatorPlayerId">The Player ID of the user initiating the creation.</param>
/// <param name="TokensToWin">Optional number of tokens required to win.</param>
public record CreateGameCommand(
    List<Guid> PlayerIds, // Changed from List<string>
    Guid CreatorPlayerId,
    int? TokensToWin
) : IRequest<Guid>; // Returns the new Game ID

public class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    // Note: Injecting IUserRepository here for validation might be less common
    // if validation primarily happens in the handler. Adjust as needed.
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.PlayerIds)
            .NotEmpty()
            .Must(list => list.Count >= 2 && list.Count <= 4).WithMessage("Game must have between 2 and 4 players.")
            .Must(list => list.Distinct().Count() == list.Count).WithMessage("Player IDs must be unique.");
        // Cannot easily validate creator inclusion or existence without repository access here.
    }
}