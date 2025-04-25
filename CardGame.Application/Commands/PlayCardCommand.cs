using CardGame.Domain.Game;
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
    Guid CardToPlayId, // Changed from Card to Guid
    Guid? TargetPlayerId,
    CardType? GuessedCardType // Pass the parsed CardType
) : IRequest; // Simple command, returns Unit (void) on success

// --- Optional: FluentValidation for Command ---
public class PlayCardCommandValidator : AbstractValidator<PlayCardCommand>
{
    public PlayCardCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.CardToPlayId).NotEmpty(); // Validate the ID is provided

        // Guard-specific validation: If guessing, guess must be valid type
        // Note: We don't know the *type* of CardToPlayId here, so this validation
        // needs to happen in the handler after loading the card.
        // RuleFor(x => x.GuessedCardType)
        //    .NotNull().When(x => x.CardToPlay.Type == CardType.Guard) // Can't do this here
        //    .WithMessage("A card type must be guessed when playing a Guard.");
        // RuleFor(x => x.GuessedCardType)
        //    .NotEqual(CardType.Guard).When(x => x.CardToPlay.Type == CardType.Guard) // Can't do this here
        //    .WithMessage("Cannot guess Guard when playing a Guard.");

        // Can still validate TargetPlayerId presence if GuessedCardType is present (implies Guard)
        RuleFor(x => x.TargetPlayerId)
            .NotEmpty().When(x => x.GuessedCardType != null)
            .WithMessage("Target player must be specified when guessing a card (Guard).");
    }
}