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

## Card Identification in the Domain

Understanding how cards are identified and compared is crucial in this domain:

1.  **Visual Identity (`AppearanceId`):**
    *   Each `Card` has an `AppearanceId` (string). This identifier is used by the presentation layer to determine how the card looks (e.g., which artwork to display).
    *   Cards that are visually identical share the same `AppearanceId`. For example, if a player has two "Guard" cards in their hand that look the same, both `Card` instances will have the same `AppearanceId`.
    *   The domain itself is not concerned with the *format* of the `AppearanceId` (e.g., it's not necessarily a URL or an asset key, but an abstract identifier for the visual representation).
    *   Standard decks typically have one unique `AppearanceId` per `CardType`. However, the design allows for future expansion decks to potentially introduce multiple `AppearanceId`s for the same functional `CardType` (e.g., alternate art versions of a Baron card).

2.  **Functional Identity (`Rank` as `CardType`):**
    *   Each `Card` has a `Rank` property, which is an instance of the `CardType` enum-like class (e.g., `CardType.Guard`, `CardType.Baron`).
    *   The `Rank` (i.e., the `CardType` instance) defines the card's behavior, rules, and its power level in the game. The integer power level is accessible via `Rank.Value`.
    *   Comparing `card1.Rank == card2.Rank` checks if two cards are functionally the same type.

3.  **Card Equality (Value Equality):**
    *   The `Card` type is a `record`. By default, records in C# implement value-based equality. This means two `Card` instances are considered equal if all their public properties have the same values.
    *   In our `Card` record, equality is therefore determined by both `AppearanceId` and `Rank` (the `CardType` instance).
    *   This value equality is critical. For example, when a player plays a card, and that card needs to be removed from their hand (`Hand.Remove(playedCard)`), the system must find and remove a card that is *equal* to the `playedCard` based on this combined visual and functional identity.
    *   This is especially important because the `playedCard` instance might be a new object (e.g., deserialized from a command) and not the exact same object instance that's in the `Hand`'s collection. `object.ReferenceEquals` would fail in such cases.
    *   `Hand.Remove(cardToRemove)` will remove the *first* card in the hand that matches `cardToRemove` based on this value equality (i.e., same `AppearanceId` and `Rank`).

4.  **No Unique Instance ID for Domain Logic:**
    *   The domain logic deliberately avoids relying on a unique `InstanceId` (like a `Guid`) for individual card instances for core gameplay mechanics like removal or comparison of played cards.
    *   The focus is on the *kind* of card (Appearance + Rank) rather than a specific, uniquely tracked instance, simplifying state management and correctly handling scenarios with deserialized objects.

This approach ensures that card comparisons are robust, especially when dealing with object instances that might originate from different sources (e.g., in-memory objects vs. deserialized objects from network requests or storage), and correctly models the user's expectation of how cards are treated in the game.
