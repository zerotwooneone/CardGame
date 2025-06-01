# CardGame.Application

## Purpose of the Project

This project acts as an orchestrator for the domain logic, handling application-level concerns and use cases. It defines the operations the application can perform (commands and queries) and contains the logic to coordinate domain objects and infrastructure services to fulfill these operations. It serves as an intermediary layer between the presentation/API layer (`CardGame.Web`) and the core domain (`CardGame.Domain`).

## Code Guidelines

### What Should Be in This Project:
*   **Commands and Command Handlers**: Operations that change the state of the system (e.g., `CreateGameCommand`, `PlayCardCommand` and their handlers). Often implemented using a pattern like CQRS with MediatR.
*   **Queries and Query Handlers**: Operations that retrieve data without changing state (e.g., `GetPlayerGameStateQuery` and its handler).
*   **Application Services**: Services that orchestrate calls to domain objects and repositories to fulfill a use case.
*   **Data Transfer Objects (DTOs)**: Objects used to transfer data between layers, especially between the Application layer and the Presentation/API layer (e.g., `GameLogEntryDto`, `PlayerDto`).
*   **Interfaces for Infrastructure Services**: Contracts for services implemented in `CardGame.Infrastructure` but required by application logic (e.g., `IPlayerNotifier`, `IDeckRepository`).

### What Should NOT Be in This Project:
*   **Core Domain Logic**: This should reside in `CardGame.Domain`. Application services should delegate to domain entities and services for business rule execution.
*   **UI-Specific Code**: Presentation logic belongs in the UI layer (`CardGame.Web/ClientApp` or API controllers in `CardGame.Web`).
*   **Direct Database Access or External Service Implementation**: These are responsibilities of `CardGame.Infrastructure`. The Application layer should use interfaces for these.
*   **ASP.NET Core specific constructs**: Controllers, middleware, etc. belong in `CardGame.Web`.

## Development Environment Setup

*   **.NET SDK**: Ensure you have the .NET SDK installed (version compatible with the project).
*   **IDE**: A C# compatible IDE like Visual Studio or Visual Studio Code with C# extensions.
*   **Building**:
    ```bash
    dotnet build CardGame.Application.csproj
    ```
    (Usually built as part of the solution build).

## Tips for AI Assistants

*   **CQRS Pattern**: This project heavily utilizes Command Query Responsibility Segregation (CQRS), often with MediatR. Look for `IRequest`, `IRequestHandler` implementations.
*   **DTOs are Key**: DTOs (`DTOs/` folder) define the data contracts for API endpoints and communication with the frontend. Understanding their structure is crucial.
*   **Orchestration, Not Logic**: Application services and handlers orchestrate domain objects. The complex business rules are in the Domain layer.
*   **Dependencies**: This project depends on `CardGame.Domain`. It defines interfaces that `CardGame.Infrastructure` implements.
*   **Key Files/Folders**:
    *   `Commands/`, `Queries/`: Folders containing command/query definitions and their handlers.
    *   `DTOs/`: Contains Data Transfer Objects.
    *   `Interfaces/`: Contains interfaces for infrastructure services.
    *   `GameEventHandlers/`: Contains handlers for domain events, often for side effects like notifications.
*   **Use Cases**: Each command/query handler typically represents a specific use case of the application.
