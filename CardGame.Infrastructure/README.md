# CardGame.Infrastructure

## Purpose of the Project

This project implements the technical and external concerns of the application. It provides concrete implementations for interfaces defined in the `CardGame.Domain` and `CardGame.Application` layers. Responsibilities include data persistence, communication with external services (like SignalR for real-time notifications), logging, and other infrastructure-level tasks.

## Code Guidelines

### What Should Be in This Project:
*   **Repository Implementations**: Concrete implementations of repository interfaces defined in `CardGame.Domain` or `CardGame.Application` (e.g., `DeckRepository` providing data access for decks). This might involve Entity Framework Core, Dapper, or other data access technologies.
*   **External Service Integrations**: Code for interacting with external services, such as SignalR Hubs (`SignalR/`), email services, payment gateways, etc.
*   **Implementations of Infrastructure Interfaces**: Concrete classes that implement interfaces like `IPlayerNotifier` (from `CardGame.Application`) or `IDeckProvider` (from `CardGame.Domain`).
*   **Logging Implementations**: Configuration and setup for logging frameworks.
*   **Dependency Injection Setup**: Extensions for `IServiceCollection` to register services specific to this layer (`DependencyInjection.cs`).

### What Should NOT Be in This Project:
*   **Core Business Logic**: This belongs in `CardGame.Domain`.
*   **Application-Level Orchestration**: This is the responsibility of `CardGame.Application`.
*   **UI or API Controllers**: These belong in `CardGame.Web`.

## Tips for AI Assistants

*   **Implementation Details**: This project contains the "how" – how data is stored, how notifications are sent, etc.
*   **Interface Implementation**: Look for classes that implement interfaces from the `Domain` and `Application` layers.
*   **External Dependencies**: This project will typically have dependencies on external libraries and SDKs (e.g., `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore.SignalR`).
*   **Configuration**: Infrastructure components often require configuration (e.g., connection strings, API keys). While the configuration values themselves are external, the code to use them resides here.
*   **Key Files/Folders**:
    *   `Persistence/`: Contains repository implementations and database context if using EF Core. (e.g., `DeckRepository.cs`)
    *   `Services/`: Contains implementations of various infrastructure services.
    *   `SignalR/`: Contains SignalR hub implementations.
    *   `DependencyInjection.cs`: Crucial for understanding how infrastructure services are registered.
*   **"Plumbing"**: This layer handles much of the application's "plumbing" and integration with the outside world.
