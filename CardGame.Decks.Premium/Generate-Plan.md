# Plan for Implementing Love Letter Premium Expansion

This document outlines the steps to implement the Love Letter Premium expansion in the `CardGame.Decks.Premium` library. This expansion will be self-contained and will not have a dependency on `CardGame.Decks.BaseGame`.

## 1. Define Premium Expansion Rules & Card Set

*   **Identify all new cards**: List each new card in the Premium expansion (e.g., Bishop, Dowager Queen, Constable, Jester, etc.).
*   **Detail card effects**: For each new card, precisely define its effect, including:
    *   Targeting rules (no target, target self, target other, target any).
    *   Conditions for the effect (e.g., if certain cards are in hand/discard).
    *   Outcome of the effect (e.g., player elimination, view card, force discard, gain token, protection).
    *   Any new game mechanics introduced (e.g., new ways to gain tokens, new win conditions if applicable).
*   **Card quantities**: Specify the number of each card in the Premium deck.
*   **Total deck size**: Confirm the total number of cards in the Premium deck.
*   **Player count considerations**: Note if any rules or card effects change based on the number of players.

## 2. Analyze `CardGame.Domain` for Necessary Changes

*   **Review `IGameOperations`**: Determine if existing methods are sufficient to implement all new card effects. If not, identify what new operations might be needed (e.g., methods to award temporary tokens, check for specific card types in discard piles, etc.).
*   **Review `Player` state**: Check if the `Player` class needs new properties to support Premium mechanics (e.g., temporary immunity types, special tokens).
*   **Review `GameLogEventType`**: Identify if new log event types are needed to accurately record the actions of Premium cards.
*   **Review `Card` and `RankDefinition`**: Ensure these can represent the new cards. The current structure should be sufficient, but confirm no new properties are needed on `RankDefinition` itself for Premium cards.
*   **Extensibility points**: Consider if any existing domain services or components need to be more extensible to accommodate the new rules without modification to their core logic (e.g., via strategy patterns or more granular interfaces).
*   **Goal**: Aim to minimize changes to `CardGame.Domain`. Prefer adding new, specific functionalities or interfaces if required, rather than altering existing core mechanics heavily relied upon by other decks.

## 3. Set Up `CardGame.Decks.Premium` Project

*   **Copy `CardRank.cs` from `BaseGame`**: Rename to `PremiumCardRank.cs`. Modify it to include all Premium card ranks with their respective values. Ensure it uses the `EnumLike` generator.
*   **Copy `DefaultDeckProvider.cs` from `BaseGame`**: Rename to `PremiumDeckProvider.cs`.
    *   Update `DeckId` to a new unique GUID for the Premium deck.
    *   Update `DisplayName` and `Description` for the Premium expansion.
    *   Update `ThemeName` (e.g., "premium") and `DeckBackAppearanceId`.
    *   Modify `GetCardQuantities()` to return the correct card counts for the Premium set using `PremiumCardRank`.
    *   Update `GetCardAppearanceId()` to point to the correct asset paths for Premium cards (e.g., `assets/decks/premium/{cardName}.webp`).
*   **Implement `ICardEffectExecutor`**: The `PremiumDeckProvider` will inherit from `BaseDeckProvider` which implements `ICardEffectExecutor`.
    *   Update the `ExecuteCardEffect` method to handle all `PremiumCardRank` values.
    *   Implement private helper methods for each Premium card's effect (e.g., `ExecuteBishopEffect`, `ExecuteJesterEffect`).
*   **Update Project File**: Ensure `CardGame.Decks.Premium.csproj` references `CardGame.Domain` and any other necessary packages (e.g., `GeneratorAttributes`).
*   **Create `README.md`**: Document the purpose, card list, and key features of the `CardGame.Decks.Premium` expansion.

## 4. Implement New Card Effects in `PremiumDeckProvider.cs`

*   For each new card in `PremiumCardRank`:
    *   Implement its logic within the corresponding `Execute[CardName]Effect` helper method.
    *   Utilize `IGameOperations` to interact with the game state (e.g., `EliminatePlayer`, `AddLogEntry`, `DrawCard`, `ProtectPlayer`).
    *   Ensure all edge cases and interactions with other cards (especially protective effects like Handmaid/Dowager Queen) are handled correctly.
    *   Add comprehensive game log entries for each step of the card's effect, including fizzles, choices made, and outcomes.

## 5. Create Unit Tests in `CardGame.Decks.PremiumTests`

*   **Copy and adapt tests from `CardGame.Decks.BaseGame.Tests`**: Use existing tests as a template.
*   **Test `PremiumDeckProvider`**: 
    *   Verify correct `DeckId`, `DisplayName`, `Description`.
    *   Verify `GetCardDeck()` returns the correct card composition and quantities for the Premium deck.
    *   Verify `GetCardAppearanceId()` returns correct paths.
*   **Test Card Effects**: For each card in `PremiumCardRank`:
    *   Create specific test methods for its effect.
    *   Use `Moq` to mock `IGameOperations` and `Player` states to isolate the card effect logic.
    *   Test all branches of logic within the card effect (e.g., success, failure, fizzle due to protection, different outcomes based on game state).
    *   Assert that the correct `IGameOperations` methods are called with the expected parameters.
    *   Assert that `GameLogEntry` events are generated correctly.
    *   Verify player state changes (elimination, protection, hand changes) as expected.
*   **Test Card Interactions**: Create tests for specific interactions between Premium cards, and between Premium cards and any existing mechanics they might affect (e.g., how a new protective card interacts with targeting cards).
*   **Update Project File**: Ensure `CardGame.Decks.PremiumTests.csproj` references `CardGame.Decks.Premium`, `CardGame.Domain`, NUnit, FluentAssertions, Moq, etc.

## 6. Integration Steps

*   **Register `PremiumDeckProvider`**: In `CardGame.Infrastructure` (or a dedicated infrastructure project for decks if it exists), ensure `PremiumDeckProvider` is registered with the Dependency Injection container so it can be resolved by `IDeckProviderFactory` or `IDeckProviderCollection`.
    *   This typically involves creating or updating an `IDeckProviderRegistrar` implementation.
*   **Update UI (if applicable)**: If there's a UI component for selecting decks, ensure the Premium deck appears as an option.

## 7. Documentation

*   **Update main project `README.md`**: Briefly mention the availability of the Premium expansion.
*   **Ensure `CardGame.Decks.Premium/README.md` is comprehensive**: It should detail the new cards, their effects, and any unique rules for the Premium expansion.

## 8. Review and Refine

*   After implementation, review the code for clarity, correctness, and adherence to existing project patterns.
*   Manually test the expansion if a playable version of the game exists.
*   Consider any performance implications of new card effects, though this is less likely to be an issue for a card game.