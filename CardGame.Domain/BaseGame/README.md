# CardGame.Domain.BaseGame

This directory contains foundational components for a "base" version or a simple expansion of the Love Letter card game. It serves as an example of how to structure an expansion library within the `CardGame.Domain` project.

## Purpose

The primary goal of this module is to provide:
1.  A concrete implementation of a standard Love Letter deck (`DefaultDeckProvider.cs`).
2.  The definition of standard card ranks used in the base game (`CardRank.cs`).
3.  A clear example of how new game content (decks, card behaviors, card ranks) can be organized and integrated into the core domain.

## Key Components

*   **`CardRank.cs`**: Defines the functional ranks of the cards in this base game (e.g., Guard, Priest, Baron, Princess). This uses an `EnumLike` structure for type safety and extensibility.
*   **`DefaultDeckProvider.cs`**: Implements the `IDeckProvider` interface from `CardGame.Domain.Interfaces` (likely by inheriting from `BaseDeckProvider`). It defines the card composition, appearance, and specific card effects for the standard Love Letter game.

## Creating Your Own Expansion

You can use this `BaseGame` module as a template for creating your own expansions or game variants:

1.  **Create a new directory** within `CardGame.Domain` (e.g., `CardGame.Domain.MyExpansion`).
2.  **Define your card ranks**: If your expansion introduces new card ranks or alters existing ones, create a `CardRank.cs` (or similar, specific to your expansion's needs) in your expansion's directory.
3.  **Implement `IDeckProvider`**: Create a new class that inherits from `BaseDeckProvider` (found in `CardGame.Domain.Providers`) or implements `IDeckProvider` directly. This class will define your expansion's unique deck, card appearances, and card effects.
4.  **Register your provider**: Ensure your new `IDeckProvider` implementation is registered with the dependency injection system. This typically involves creating an `IDeckProviderRegistrar` in your infrastructure project that adds your provider to the `IDeckProviderCollection`.

By following this pattern, you can extend the game with new themes, mechanics, and cards while keeping the domain logic modular and organized.