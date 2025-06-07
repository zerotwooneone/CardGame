using CardGame.Domain.Types;
using FluentValidation;
using MediatR;

namespace CardGame.Application.Commands;

/// <summary>
/// Command to play a card in a game.
/// </summary>
public record PlayCardCommand(
    Guid GameId,
    Guid PlayerId, // ID of the player making the move (authenticated user)
    string CardToPlayId, // Changed from Card to Guid
    Guid? TargetPlayerId,
    CardRank? GuessedCardType

    // Pass the parsed CardType
) : IRequest; // Simple command, returns Unit (void) on success

// --- Optional: FluentValidation for Command ---
public class PlayCardCommandValidator : AbstractValidator<PlayCardCommand>
{
    public PlayCardCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.CardToPlayId).NotEmpty(); // Validate the ID is provided

        // Can still validate TargetPlayerId presence if GuessedCardType is present (implies Guard)
        RuleFor(x => x.TargetPlayerId)
            .NotEmpty().When(x => x.GuessedCardType != null)
            .WithMessage("Target player must be specified when guessing a card (Guard).");
    }
}