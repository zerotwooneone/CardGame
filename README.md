# Love Letter Card Game - DDD & Code Generation Exploration

This project is a C# and Angular implementation of the popular card game "Love Letter". Its primary purpose is to serve as a learning exercise focused on several key software development concepts:

* **Domain-Driven Design (DDD):** Exploring how to model the game's rules and concepts within a dedicated domain layer, keeping business logic separate from application and infrastructure concerns. The goal is to create a rich domain model where invariants are enforced within the aggregate root (`Game`).
* **Clean Architecture Principles:** Structuring the backend into distinct layers (Domain, Application, Infrastructure, Web API) to promote separation of concerns, testability, and maintainability.
* **Code Generation:** Investigating the use of C# Source Generators to reduce boilerplate code, specifically demonstrated through the implementation of type-safe "Smart Enums" (like `CardType`, `PlayerStatus`) using the `EnumLike` pattern.
* **Modern Web Development:** Utilizing Angular (v19) with standalone components, Signals, Material components, and SCSS for the frontend client, structured by feature.
* **Containerization:** Using Docker to package the backend API and the built Angular frontend into a single, runnable image.

## Technologies Used

* **Backend:** .NET 9.0, ASP.NET Core, MediatR (for CQRS), FluentValidation, NUnit, Moq
* **Frontend:** Angular 19 (Standalone Components), Angular Material, TypeScript, SCSS, SignalR Client
* **Containerization:** Docker
* **Code Generation:** C# Source Generators

## Project Structure (Conceptual)

### Backend (.NET) - Package by Layer

* **`CardGame.Domain`:** Contains the core game logic, entities (`Game`, `Player`), value objects (`Card`, `Hand`, `Deck`), domain events, custom exceptions, and domain interfaces (`IGameRepository`, `IUserRepository`, `IDomainEventPublisher`). Uses generated "EnumLike" classes for types (`CardType`, etc.). Designed to have no dependencies on Application or Infrastructure layers.
* **`CardGame.Application`:** Orchestrates use cases. Contains Commands, Queries, Handlers (using MediatR), DTOs, validation logic (FluentValidation), and application-level interfaces (`INotificationService`, `IGameStateBroadcaster`, `IUserAuthenticationService`). Depends on Domain.
* **`CardGame.Infrastructure`:** Implements interfaces defined in lower layers. Contains repository implementations (`InMemoryGameRepository`, `InMemoryUserRepository`), SignalR notification/broadcaster implementations, and potentially database access logic (e.g., EF Core DbContext) in a real-world scenario. Depends on Application and Domain.
* **`CardGame.Web`:** The ASP.NET Core Web API project. Contains API Controllers, SignalR Hubs (`NotificationHub`, `GameHub`), `Program.cs` for setup (DI, middleware), DTOs specific to API requests/responses, and serves the built Angular frontend. Depends on Application and Infrastructure.

### Frontend (Angular) - Package by Feature

* **`core/`:** Singleton services (`AuthService`, `SignalrService`), guards, interceptors, shared DTO models.
* **`features/`:** Contains folders for distinct application features (e.g., `auth`, `lobby`, `game`). Each feature folder contains its specific components, services, and potentially routes.
* **`shared/`:** Reusable UI components, directives, pipes across different features.

## Running with Docker

This project includes a multi-stage `Dockerfile` to build and run the combined backend and frontend application in a container.

**1. Build the Docker Image:**

Navigate to the **root directory of your solution** (where the `.sln` file and the `Dockerfile` are located) in your terminal and run:

```bash
docker build -t cardgame-app -f Dockerfile .
```

* `-t cardgame-app`: Tags the built image with the name `cardgame-app`. You can choose a different tag.
* `-f Dockerfile`: Specifies the Dockerfile to use (usually redundant if named `Dockerfile`).
* `.`: Specifies the build context (the current directory).

**2. Run the Docker Container:**

Once the image is built, run it using:

```bash
docker run --rm -it -p 8080:8080 -e ASPNETCORE_URLS=http://+:8080 --name cardgame-container cardgame-app
```

* `--rm`: Automatically removes the container when it stops.
* `-it`: Runs the container interactively so you can see logs and stop it with Ctrl+C. Use `-d` instead to run detached (in the background).
* `-p 8080:8080`: **Maps port 8080 on your host machine** to port 8080 inside the container. Your browser/Postman will connect to `http://localhost:8080`.
* `-e ASPNETCORE_URLS=http://+:8080`: **Explicitly tells the ASP.NET Core application inside the container** to listen for HTTP traffic on port 8080. This is often necessary for containerized applications.
* `--name cardgame-container`: Assigns a convenient name to the running container.
* `cardgame-app`: The name of the image you built.

**3. Access the Application:**

Open your web browser and navigate to: `http://localhost:8080`

## AI Assistant Guidelines

This section is intended for AI coding assistants (like Cascade) to ensure smooth and effective collaboration. Please adhere to these guidelines:

1.  **Code Comments:**
    *   **Do not** leave comments in the code that merely describe the changes you have made (e.g., "Added null check," "Refactored to use service"). This is what commit messages are for.
    *   **Do** leave comments that a Senior Software Engineer would approve of. This includes comments that explain *why* a certain complex or non-obvious piece of logic exists, document important pre-conditions or post-conditions, or clarify potential edge cases.
2.  **Updating These Guidelines:**
    *   If you learn something new about the project structure, key architectural decisions, or common pitfalls that would be beneficial for future AI interactions, please **proactively update this "AI Assistant Guidelines" section** in `README.md`.
3.  **Project Structure & Key Files:**
    *   The core domain logic for the game is encapsulated in the `Game` class, located at `CardGame.Domain\Game\Game.cs`. Its fully qualified name is `CardGame.Domain.Game.Game`.
    *   Dependency Injection for the application layer is configured in `CardGame.Application\DependencyInjection.cs` within the `AddApplicationServices` extension method.
    *   Interfaces are generally placed in the `Interfaces` subfolder of the project they belong to (e.g., `CardGame.Domain\Interfaces`).
    *   **Architectural Preference:** Where sensible, aim to organize code by feature (e.g., grouping all files related to 'player authentication' together) rather than strictly by technical type (e.g., all 'controllers' in one folder, all 'services' in another). This is already followed in the Angular frontend (`features/`) and should be a consideration for backend organization where appropriate.
4.  **General Approach:**
    *   Prioritize understanding the existing architecture and patterns before introducing new ones.
    *   When refactoring, ensure all related tests are updated and pass.
    *   If unsure about an approach, ask for clarification rather than making assumptions.
    * After making code changes, ask to recompile and run tests.
5.  **Angular State Management:**
    *   When managing reactive state in Angular services or components, **prefer to use Angular Signals over BehaviorSubjects/RxJS Subjects** where appropriate. Signals offer a more fine-grained reactivity model and are becoming the recommended approach for many state management scenarios in modern Angular.
6.  **Memory & Context:**
    *   Pay close attention to provided memories and checkpoint summaries. They contain critical context about previous decisions and current objectives.
    *   Proactively create new memories for significant learnings, architectural decisions, or user preferences to aid future sessions.

## A Note on Collaboration

While the initial structure and concepts for this project were outlined manually, the vast majority of the C# backend code (domain logic, application layer, infrastructure implementations, tests), Angular frontend components and services, Dockerfile configuration, and associated documentation were generated and iteratively refined through collaboration with **Gemini 2.5 Pro**. It's been an interesting experiment in AI-assisted development for exploring DDD and related patterns! This really helped knock out some of the tedious parts of clean architecture and assisted with refactoring things quickly.
