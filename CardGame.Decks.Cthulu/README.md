# CardGame.Decks.Cthulu

This project provides a Cthulhu Mythos-themed re-skin for the Love Letter card game. It leverages the core game mechanics and card definitions from the `CardGame.Decks.BaseGame` project, applying a new visual theme.

## Purpose

The primary goal of this module is to:
1.  Offer an alternative visual experience for the standard Love Letter game, themed around the Cthulhu Mythos.
2.  Demonstrate how a new theme can be applied to an existing deck provider by inheriting and overriding appearance-specific properties and methods.

## Key Components

*   **`CthulhuDeckProvider.cs`**:
    *   Inherits from `DefaultDeckProvider` (found in the `CardGame.Decks.BaseGame` project).
    *   Overrides properties such as `DeckId`, `DisplayName`, `Description`, `ThemeName`, and `DeckBackAppearanceId` to reflect the Cthulhu theme.
    *   Overrides the `GetCardAppearanceId` method to provide paths to Cthulhu-themed card artwork (e.g., `assets/decks/cthulu/guard.webp`).
    *   Uses the existing `CardRank` definitions from `CardGame.Decks.BaseGame` for card functionality and mechanics. No new card types or game rules are introduced by this provider.

## How It Works

The `CthulhuDeckProvider` reuses the entire card set, quantities, and game logic defined in the `DefaultDeckProvider` from the base game. Its primary function is to map the standard card ranks to new, Cthulhu-themed visual appearances. This allows players to experience the familiar Love Letter gameplay with a different aesthetic.

## Integration

To use this Cthulhu-themed deck:
1.  Ensure this `CardGame.Decks.Cthulu` project is referenced by your main application or infrastructure project.
2.  The `CthulhuDeckProvider` needs to be registered with the dependency injection system, typically by adding it to an `IDeckProviderCollection` (similar to how `DefaultDeckProvider` is registered). This will make the "Cthulhu Mythos" deck available for selection in the game.

This module serves as an example of how to create thematic variations of existing game decks without altering the core gameplay mechanics.
