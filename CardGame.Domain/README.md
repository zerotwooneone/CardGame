# CardGame.Domain

## Purpose of the Project

This project is the heart of the Love Letter card game application. It contains the core business logic, entities, value objects, domain events, and domain services. The primary goal of this project is to model the game's rules and state in a way that is independent of any specific technology or infrastructure. It embodies the "Ubiquitous Language" of the game.

## Code Guidelines

### What Should Be in This Project:
*   **Entities**: Core domain objects with identity and lifecycle (e.g., `Game`, `Player`, `Card`).
*   **Value Objects**: Immutable objects that describe attributes (e.g., `CardType` if it were a complex value object, though currently it's an enum).
*   **Domain Events**: Records of significant occurrences within the domain (e.g., `PlayerPlayedCard`, `RoundStarted`).
*   **Domain Services**: Logic that doesn't naturally fit within a single entity.
*   **Aggregate Roots**: Entities that act as entry points to a cluster of related objects.
*   **Repository Interfaces**: Contracts for data persistence, defined here but implemented in the `CardGame.Infrastructure` project (e.g., `IDeckRegistry`).
*   **Enums**: Domain-specific enumerations like `CardType`.

### What Should NOT Be in This Project:
*   Infrastructure-specific code (e.g., database access logic, API client libraries, file system operations).
*   UI logic or presentation concerns.
*   Application-level services or command/query handlers (these belong in `CardGame.Application`).
*   Direct dependencies on other projects within this solution, except potentially a shared kernel if one existed. It should be highly independent.

## Development Environment Setup

*   **.NET SDK**: Ensure you have the .NET SDK installed (version compatible with the project, likely .NET 6.0 or newer). You can download it from [the official .NET website](https://dotnet.microsoft.com/download).
*   **IDE**: A C# compatible IDE like Visual Studio or Visual Studio Code with C# extensions.
*   **Building**:
    ```bash
    dotnet build CardGame.Domain.csproj
    ```
    (Usually built as part of the solution build from the root or `CardGame.Web` project).

## Tips for AI Assistants

*   **Ubiquitous Language**: This project defines the core terms and concepts of the game. Pay close attention to class and method names.
*   **Key Files**:
    *   `Game.cs`: Central class orchestrating game flow, rules, and state.
    *   `Card.cs`: Represents a card in the game.
    *   `Player.cs`: Represents a player in the game.
    *   `Events/`: Folder containing domain event classes.
    *   `Interfaces/`: Folder containing domain-specific interfaces like `IDeckProvider` and `IDeckRegistry`.
*   **DDD Principles**: This project adheres to Domain-Driven Design (DDD) principles. Focus on pure business logic and rules.
*   **Impact of Changes**: Modifications here can have significant ripple effects throughout the application.
*   **Purity**: Strive to keep this layer free from infrastructure or application-specific concerns.
*   **No External Dependencies**: This project should have minimal external library dependencies, usually only core .NET libraries.

## Key Domain Concepts

### The Game Log as a Narrative

A crucial concept in this domain is the **Game Log**. It is not a technical or system-level debug log. Instead, its primary purpose is to create a clear, human-readable narrative of the game's events for players and spectators.

*   **Purpose**: To tell the story of the game round, explaining *what* happened and *why*.
*   **Audience**: Players and spectators.
*   **Clarity**: Log messages should be descriptive and easy to understand, especially for events that cause sudden or non-obvious state changes (e.g., a player being eliminated, the round ending, a card's effect being fizzled).
*   **`GameLogEventType`**: The `GameLogEventType` enum should be used to create distinct, specific story beats. This allows the UI to potentially render different types of events in unique ways (e.g., with different icons or styling) to enhance the narrative quality.

## Card Identification in the Domain

The domain has a specific way of identifying and comparing cards, which is crucial for game logic, especially when dealing with card effects, hand management, and potentially network synchronization or persistence if those features were fully implemented.

1.  **`AppearanceId` (string):** This is primarily for the presentation layer. It's a string (e.g., `"guard"`, `"premium-assassin"`) that the UI can use to display the correct card image and potentially basic text. Multiple actual card *instances* in a game can share the same `AppearanceId` (e.g., there are multiple Guard cards in a deck).

2.  **`Rank` (int):** This is an integer representing the card's functional value or power level. For example, a Guard might be Rank 1, a Priest Rank 2, etc. This is distinct from `AppearanceId` because different decks (e.g., base game vs. premium expansion) might have cards with the same numerical rank but different names and effects (and thus different `AppearanceId`s).
    *Deck providers (implementations of `IDeckProvider`) are responsible for interpreting a card's `Rank` (integer) and `AppearanceId` (string) to resolve it to a specific, rich card type object (like `PremiumCardRank` or a base game `CardRank` type). This rich type contains the detailed effect logic, name, description, etc.*

3.  **`Card` Equality:** The `Card` type itself (`CardGame.Domain.Game.Card`) is a `record`. This means equality is value-based, determined by its properties: `Rank` (int) and `AppearanceId` (string). Two `Card` objects are considered equal if they have the same `Rank` and the same `AppearanceId`. This is vital for operations like `Hand.Remove(playedCard)`, because the `playedCard` instance might be a new object (e.g., created from a player's action or deserialized) and not the same reference as the one in the hand list.

4.  **No Unique Instance ID (for core gameplay):** Critically, the domain logic for core gameplay (playing cards, effects, hand management) deliberately *avoids* relying on a unique `InstanceId` (like a `Guid`) for individual card *instances*. The game logic cares about the *kind* of card being played (defined by its `AppearanceId` and `Rank`), not a specific, uniquely tracked instance of that card. This simplifies state management, especially if cards are ever serialized/deserialized, and aligns with how physical card games work (you don't track each Guard card by a unique serial number).
    -   While an `InstanceId` *was* briefly and incorrectly part of the `Card` record, it has been removed to adhere to this domain principle. The identity and behavior are fully defined by `Rank` and `AppearanceId`.

This approach ensures that the game logic is robust and that card identity is based on its functional and visual characteristics rather than a specific object reference or unique ID, which is more aligned with the nature of card games.

## Domain Events
