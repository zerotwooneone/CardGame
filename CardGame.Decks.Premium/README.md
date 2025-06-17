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

## Logging Philosophy

The `ILogger` interface provided via dependency injection (e.g., `ILogger<PremiumDeckProvider>`) is used for internal diagnostics and operational visibility within the `PremiumDeckProvider`. This logging is distinct from `GameLogEntry` events, which are intended for player-facing game history and potentially for game replays or audits.

Guidance for using `ILogger` levels within this project:

-   **`LogError`**: Should be used very rarely. Reserved for catastrophic failures within the provider that, for some reason, don't warrant an immediate exception but indicate a severe, unrecoverable problem (e.g., failure to initialize critical static data if it weren't handled by a static constructor, or a caught exception from a critical infrastructure piece that still allows the provider to limp along in a degraded state – though exceptions are generally preferred).

-   **`LogWarning`**: For conditions that might lead to unexpected behavior but are somewhat recoverable or have been handled, or that indicate a potential misuse of the provider by its caller. 
    *   Example: If the game engine calls `ExecuteCardEffect` with parameters that violate a card's known preconditions (e.g., no target provided for a card that requires one, attempting to target self with a card that prohibits it). The effect will typically fizzle, and a `GameLogEntry` will record this, but the `LogWarning` highlights a potential issue in the calling logic.

-   **`LogInformation`**: For high-level, summary-type information about the provider's lifecycle or significant state changes that are not part of routine card effect execution. This level should be used sparingly to avoid excessive noise.
    *   Example: "PremiumDeckProvider initialized with X unique card types."

-   **`LogDebug`**: This is the primary level for detailed operational flow during card effect execution. Use this to trace the steps taken, decisions made, and intermediate states within an effect's logic.
    *   Examples: "Executing Guard effect for Player A on Player B", "Player B discarded Princess due to Prince effect", "Target C is protected by Handmaid, fizzling effect."

-   **`LogTrace`**: For extremely verbose, low-level diagnostic information. This might include logging every minor step, data value, or frequent event if needed for intensive debugging scenarios. Use with caution as it can generate a very large volume of logs.

**Key Distinction**: Always remember that `ILogger` is for the *provider's internal state and diagnostics*. Player-visible game events, rule outcomes, and actions taken are logged via `gameOperations.AddLogEntry(new GameLogEntry(...))`. Exceptions are the preferred way to handle contract violations or unrecoverable internal errors.

## AI Assistant Notes

- When modifying card effects, ensure consistency between `PremiumCardRank.cs` (properties like `RequiresTarget`, `Description`), `PremiumDeckProvider.cs` (effect execution logic), and `CardEffects.md` (specification).
- The `PremiumDeckProvider.GetPremiumRankFromCard()` method is crucial for correctly interpreting a generic `Card` object from the domain as a specific `PremiumCardRank`.
- **Error Handling**: Never use `gameOperations.AddLogEntry` to log internal system errors, bugs, or unhandled cases. The `GameLog` is for player-facing events only. For programming errors (like a missing card effect implementation), **throw an exception** (e.g., `NotImplementedException`) to ensure the issue is surfaced immediately and not hidden in game logs.
