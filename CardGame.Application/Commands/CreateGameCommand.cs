using FluentValidation;
using MediatR;

namespace CardGame.Application.Commands;

/// <summary>
/// Command to create a new game.
/// </summary>
/// <param name="PlayerIds">Player IDs of players joining (2-4).</param> // Changed parameter name
/// <param name="CreatorPlayerId">The Player ID of the user initiating the creation.</param>
/// <param name="DeckId">The ID of the deck to use for this game.</param> // Added DeckId parameter
/// <param name="TokensToWin">Optional number of tokens required to win.</param>
public record CreateGameCommand(
    List<Guid> PlayerIds, // Changed from List<string>
    Guid CreatorPlayerId,
    Guid DeckId, // Added DeckId property
    int? TokensToWin
) : IRequest<Guid>; // Returns the new Game ID

public class CreateGameCommandValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameCommandValidator()
    {
        RuleFor(x => x.PlayerIds)
            .NotEmpty().WithMessage("At least one player ID must be provided.")
            .Must(ids => ids.Count >= 2 && ids.Count <= 4).WithMessage("Game must have between 2 and 4 players.")
            .ForEach(idRule => idRule.NotEmpty().WithMessage("Player ID cannot be empty."));

        RuleFor(x => x.CreatorPlayerId)
            .NotEmpty().WithMessage("Creator player ID must be provided.")
            .Must((command, creatorId) => command.PlayerIds?.Contains(creatorId) ?? false)
            .WithMessage("Creator player ID must be one of the player IDs in the list.");

        RuleFor(x => x.DeckId) // Added validation for DeckId
            .NotEmpty().WithMessage("Deck ID must be provided.");

        RuleFor(x => x.TokensToWin)
            .GreaterThanOrEqualTo(1).When(x => x.TokensToWin.HasValue)
            .WithMessage("Tokens to win must be at least 1.");
    }
}