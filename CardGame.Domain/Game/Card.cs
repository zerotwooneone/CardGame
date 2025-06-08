using CardGame.Domain.Types;
using CardRank = CardGame.Domain.BaseGame.CardRank;

namespace CardGame.Domain.Game;

/// <summary>
/// Represents a card in the game. A card is defined by its visual appearance and its functional type (which also determines its rank).
/// It behaves as a Value Object, meaning instances with the same <see cref="AppearanceId"/> and <see cref="Rank"/> (which is the <see cref="CardRank"/> instance)
/// are considered equal. This is crucial for operations like removing a card from a hand, especially
/// when dealing with deserialized instances where reference equality cannot be relied upon.
/// </summary>
public record Card // Using record for immutability and value equality
{
    /// <summary>
    /// Gets the identifier for the card's visual appearance.
    /// This identifier allows the presentation layer to display the correct card art.
    /// Cards that look the same share the same <see cref="AppearanceId"/>.
    /// Multiple distinct card instances in a game (e.g., two 'Guard' cards in a hand) will have the same <see cref="AppearanceId"/> if they are visually identical.
    /// Expansion decks might introduce different <see cref="AppearanceId"/>s for the same functional <see cref="CardRank"/>.
    /// The domain is not concerned with the specific format of this ID (e.g., URL, asset key).
    /// </summary>
    public string AppearanceId { get; }

    /// <summary>
    /// Gets the functional type of the card (e.g., Guard, Priest, Baron), which dictates its behavior and rules in the game.
    /// The <see cref="CardRank.Value"/> property of this Rank provides the integer power level of the card.
    /// See <see cref="CardRank"/> for all available types.
    /// </summary>
    public CardRank Rank { get; } // This property holds the CardType, its .Value is the integer rank.

    /// <summary>
    /// Initializes a new instance of the <see cref="Card"/> record.
    /// </summary>
    /// <param name="appearanceId">The identifier for the card's visual appearance. Cannot be null or empty.</param>
    /// <param name="rank">The functional type of the card (which also defines its rank). Cannot be null. This will be assigned to the <see cref="Rank"/> property.</param>
    public Card(string appearanceId, CardRank rank)
    {
        if (string.IsNullOrEmpty(appearanceId)) throw new ArgumentNullException(nameof(appearanceId));
        AppearanceId = appearanceId;
        Rank = rank ?? throw new ArgumentNullException(nameof(rank)); // 'type' is the CardType, assigned to Rank
    } 
}