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
