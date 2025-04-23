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
    Card CardToPlay, // The actual Card instance being played
    Guid? TargetPlayerId,
    CardType? GuessedCardType // Pass the parsed CardType
) : IRequest; // Simple command, returns Unit (void) on success

// Note: Some validation (like checking if player holds the card)
// is better done in the handler where game state is loaded.
public class PlayCardCommandValidator : AbstractValidator<PlayCardCommand>
{
    public PlayCardCommandValidator()
    {
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.CardToPlay).NotNull();
        // Guard-specific validation: If playing a Guard, guess must be valid
        RuleFor(x => x.GuessedCardType)
            .NotNull().When(x => x.CardToPlay.Type == CardType.Guard)
            .WithMessage("A card type must be guessed when playing a Guard.");
        RuleFor(x => x.GuessedCardType)
            .NotEqual(CardType.Guard).When(x => x.CardToPlay.Type == CardType.Guard)
            .WithMessage("Cannot guess Guard when playing a Guard.");
        // Target validation based on card type can also be added here or checked in handler
    }
}