# CardGame.Domain.SystemTests

This project contains system tests for the `CardGame.Domain` library. The primary purpose of these tests is to simulate complete game plays to validate the core domain logic, game progression, rule enforcement, and overall stability of the Love Letter game implementation under various scenarios.

## Key Focus Areas for Tests

*   **Full Game Lifecycle Simulation:** Tests aim to simulate entire games from start to finish, including multiple rounds and turns.
*   **Game Progression & Completion:** Verifying that games and rounds start, progress, and end correctly according to the game rules (e.g., last player standing, required tokens won).
*   **Randomized Gameplay & Reproducibility:** Leveraging a custom `TestRandomizer` to introduce controlled randomness in player decisions (card play, target selection). Each test run, especially in parallel, uses independent seeds which are logged extensively. This allows for the exact reproduction of any failing game scenario by re-running a test with the specific seed that caused an issue.
*   **Domain Exception Handling:** Actively trying to surface and catch domain-specific exceptions (like `GameRuleException`, `InvalidMoveException`) thrown by `CardGame.Domain`. Detailed logging, including the random seed and player states, is captured to help diagnose and pinpoint potential bugs or edge cases in the domain logic.
*   **Parallel Execution & Thread Safety:** Tests are designed to be run in parallel to uncover concurrency issues, with each game simulation being self-contained and using its own `TestRandomizer` instance.

## Technologies Used

*   **.NET 9.0** (or the current version used by the solution)
*   **NUnit:** As the primary testing framework.
*   **FluentAssertions:** For writing expressive and readable assertions.

## How to Run Tests

Tests can be executed through:

*   **Visual Studio Test Explorer:** Discover and run tests directly within the IDE.
*   **Command Line:** Using the `dotnet test` command in the terminal, typically navigated to the solution root or the `CardGame.Domain.SystemTests` project directory.
    ```bash
    # From solution root
    dotnet test --filter "TestCategory=CardGame.Domain.SystemTests"

    # Or from the project directory
    dotnet test
    ```

## Important Notes

*   **High-Level Validation:** These system tests focus on the overall game flow, critical rule enforcement (e.g., card effects, player elimination, round/game end conditions), and the integrity of the `Game` aggregate. They are not intended to exhaustively check every possible game state permutation after each individual turn, as unit tests in `CardGame.Domain.Tests` cover more granular logic.
*   **Seed-Based Debugging:** When a test fails, the logged seed is crucial for debugging. The `Should_Successfully_Complete_Specific_Game_With_Random_Plays` test method can be used by providing it the failing seed to deterministically reproduce the scenario.
*   **Test AI Limitations:** The 'AI' within the tests for selecting cards and targets is designed to make valid moves where possible. If it encounters a situation where no valid move can be made according to its logic (and the game rules), it may throw an assertion to highlight this, which is distinct from a domain exception.
