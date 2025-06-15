# CardGame.Decks.Premium

This project provides the "Premium" deck for the CardGame, an expanded version of the classic Love Letter game with additional roles and more complex card interactions.

## Purpose

The primary purpose of this project is to define and supply the Premium deck, including:
- The set of unique premium card ranks and their properties.
- The logic for executing the effects of each premium card.
- A concrete implementation of the `IDeckProvider` interface from `CardGame.Domain` tailored for this premium deck.

## Key Files

- **`PremiumCardRank.cs`**: Defines the `PremiumCardRank` sealed class. Each static readonly instance represents a unique card in the premium deck (e.g., Jester, Assassin, Cardinal, Bishop). It includes properties for `Id` (Guid), `Name` (string), `Value` (int, the card's numerical rank), `QuantityInDeck` (int), `Description` (string, effect text), `RequiresTarget` (bool), and `CanTargetSelf` (bool).

- **`PremiumDeckProvider.cs`**: Contains the `PremiumDeckProvider` class, which implements the `IDeckProvider` interface. This class is responsible for:
    - Providing the `DeckDefinition` for the Premium deck, which includes a list of all `Card` instances in the deck.
    - Implementing the `ExecuteCardEffect` method, which contains the detailed logic for how each premium card's effect is resolved within the game.
    - Mapping domain `Card` objects (which have an `int Rank` and `string AppearanceId`) to their specific `PremiumCardRank` to determine behavior.

- **`CardEffects.md`**: A markdown document detailing the effects, quantities, and any special rules for each card in the Premium deck. This document serves as the specification for the card behaviors implemented in `PremiumDeckProvider.cs` and properties in `PremiumCardRank.cs`.

## Deck Composition

The Premium deck consists of the following cards (Name, Value, Quantity):

- Jester (0) x1
- Assassin (0) x1
- Guard (1) x8
- Priest (2) x2
- Cardinal (2) x2
- Baron (3) x2
- Baroness (3) x2
- Handmaid (4) x2
- Sycophant (4) x2
- Prince (5) x2
- Count (5) x2
- King (6) x1
- Constable (6) x1
- Countess (7) x1
- Dowager Queen (7) x1
- Princess (8) x1
- Bishop (9) x1

Total cards: 32

## Integration

To use the Premium deck in the game:
1. Ensure the `CardGame.Decks.Premium` project is referenced by the application's composition root (e.g., `CardGame.Application` or the main executable project).
2. Register `PremiumDeckProvider` with the dependency injection container as an implementation of `IDeckProvider`.
3. The game can then request this deck via its `DeckId` (A9A5F11F-4760-4A2C-9B6C-4F9F1CF9A717) or allow users to select it.

The `AppearanceId` for cards from this deck will follow the format `premium-<card-name-slug>` (e.g., `premium-jester`, `premium-dowager-queen`).

## Coding and Error Handling Philosophy

While this project, particularly `PremiumDeckProvider.cs`, implements domain interfaces (like `IDeckProvider`) and encapsulates complex card effect logic, it's crucial to maintain robustness and clarity in its behavior, especially concerning error states.

- **Domain-Like Rigor**: The code within `PremiumDeckProvider.cs` should be treated with a rigor similar to core domain logic. It translates abstract card plays into concrete game actions.
- **Exception-Based Error Handling for Internal Issues**: For situations that indicate an internal inconsistency, a programming error, a violation of fundamental preconditions (e.g., receiving a `Card` object that cannot be resolved to a known `PremiumCardRank`), or an impossible state, the provider should **throw an appropriate exception** (e.g., `InvalidOperationException`, `ArgumentNullException`, `ArgumentOutOfRangeException`).
    - This approach ensures that such problems are surfaced immediately and loudly, preventing the game from continuing in a potentially corrupted or unpredictable state.
    - It contrasts with "graceful degradation" (e.g., silently logging an error and attempting to continue or simply "fizzling" an effect due to an internal bug). Such silent failures can mask deeper issues.
- **Distinction from Game Rule "Fizzles"**: This philosophy is distinct from handling expected game rule outcomes where a card effect doesn't fully resolve due to player choices or game state (e.g., a Guard's guess is incorrect, a player targets an immune player). These are normal game events and should be handled by returning appropriate results and logging `GameLogEventType.EffectFizzled` as per existing logging conventions, not by throwing exceptions.
- **Clarity and Maintainability**: Adopting an exception-first approach for unexpected internal errors makes the codebase more maintainable and easier to debug, as issues are not hidden.

This ensures that the `PremiumDeckProvider` acts as a reliable and predictable component in translating card plays into game effects, failing fast when its own operational integrity is compromised.

## AI Assistant Notes

- When modifying card effects, ensure consistency between `PremiumCardRank.cs` (properties like `RequiresTarget`, `Description`), `PremiumDeckProvider.cs` (effect execution logic), and `CardEffects.md` (specification).
- The `PremiumDeckProvider.GetPremiumRankFromCard()` method is crucial for correctly interpreting a generic `Card` object from the domain as a specific `PremiumCardRank`.
