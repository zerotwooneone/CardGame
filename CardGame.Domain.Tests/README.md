# CardGame.Domain.Tests

This project contains unit tests for the `CardGame.Domain` library. Its primary purpose is to verify the correctness of the core domain logic, entities, and value objects of the Love Letter card game.

## Purpose and Scope

The tests in this project focus on:

*   **Core Domain Logic:** Verifying the behavior of domain classes such as `Game`, `Player`, `Hand`, `Deck`, and other related components.
*   **State Transitions:** Ensuring correct state changes within the `Game` aggregate (e.g., game setup, turn progression, player elimination, round end, game end).
*   **Invariant Enforcement:** Confirming that domain invariants (rules that must always hold true) are maintained.
*   **Domain Event Emission:** Checking that appropriate domain events are raised at the correct times.

**Limited Scope:**
It is crucial to understand that these tests **do not cover the specific effects or outcomes of playing individual cards** (e.g., the detailed logic of what happens when a Guard, Priest, or Baron is played). Such tests, which involve card-specific behaviors defined by deck implementations, belong in the test projects associated with those specific deck packages (e.g., tests for `CardGame.Decks.BaseGame` would cover its card effects). This project strictly tests the foundational domain mechanics.

## Key Components

*   **`GameTests.cs`**: Contains the majority of tests, focusing on the `Game` aggregate and its interactions.
*   **`TestDoubles/`**: This directory contains test doubles (fakes, stubs, mocks) used to isolate the domain logic from external dependencies or to provide controlled test data. Key fakes include:
    *   `NonShufflingRandomizer.cs`: Provides a predictable implementation of `IRandomizer` for consistent test outcomes.
    *   `FakeLogger.cs` & `FakeLoggerFactory.cs`: Provide no-op logging implementations for `ILogger<T>` and `ILoggerFactory`, preventing logging side effects during tests.
    *   `FakeDeckProvider.cs`: A configurable implementation of `IDeckProvider`, allowing tests to specify card sets, rank definitions, and optionally hook into card effect execution logic.

### Important Testing Considerations

*   **`GamePhase` Testing**: Be aware that methods like `Game.CreateNewGame()` and `Game.PlayCard()` can trigger subsequent game logic that advances turns, starts new rounds, or even ends the game. This means that the `GamePhase` might change to `RoundOver` or `GameOver` immediately within the execution of these methods. When testing, focus on the expected outcomes of these state transitions (e.g., correct winner awarded, new round setup correctly, game ended appropriately) rather than assuming `GamePhase` will remain `InProgress` unless the test conditions are specifically designed to prevent phase changes.

## Testing Dependencies

This project utilizes the following libraries for testing:

*   **NUnit**: The primary testing framework.
*   **FluentAssertions**: For writing more readable and expressive assertions.
*   **Moq**: For creating mock objects to isolate units under test.
*   **Microsoft.Extensions.Logging.Console**: For capturing log output during tests if needed.
*   **Microsoft.NET.Test.Sdk**: Provides the MSBuild infrastructure for running tests.
*   **coverlet.collector**: For collecting code coverage information.

## AI Assistant Guidelines for This Project

When contributing to or modifying tests in this project, please adhere to the following:

1.  **Focus on Domain Logic:** Ensure new tests specifically target the core logic and invariants within `CardGame.Domain`.
2.  **Respect Scope:** Do not add tests here for individual card play effects. Place those in the test projects for the relevant deck implementations (e.g., `CardGame.Decks.BaseGame.Tests`).
3.  **Isolation:** Use mocks (via Moq) or stubs for any dependencies defined by interfaces in `CardGame.Domain.Interfaces` (like `IDeckProvider`, `IGameRepository`) to ensure tests are true unit tests of the domain logic.
4.  **Clarity:** Write clear and descriptive test method names that explain what is being tested and the expected outcome. Use FluentAssertions to make assertions easy to understand.
5.  **Coverage:** When adding new features or modifying existing logic in `CardGame.Domain`, ensure corresponding unit tests are added or updated in this project to maintain good test coverage of the domain rules.
6.  **No Side Effects:** Tests should be self-contained and not rely on external state or produce side effects that could interfere with other tests.
7.  **README Updates:** If significant changes are made to the testing strategy, scope, or tooling for this project, please proactively update this README.md.

By following these guidelines, we can maintain a robust and focused test suite for the core domain of the CardGame application.
