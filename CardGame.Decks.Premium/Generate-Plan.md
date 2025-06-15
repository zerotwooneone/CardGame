# Plan for AI-Driven Implementation of Love Letter Premium Expansion

**ATTENTION AI ASSISTANT:** This document outlines a step-by-step plan for you to implement the Love Letter Premium expansion. Execute each task sequentially. **CRITICAL: Before generating or modifying any code, you MUST verify the existence and exact names/signatures/namespaces of all classes, methods, properties, and file paths by using tools like `grep_search`, `view_file`, `list_dir`, or `codebase_search`. DO NOT MAKE ASSUMPTIONS OR GUESS. Confirm each step's completion with the USER before proceeding to the next.**

## Phase 1: Foundation and Information Gathering

### Task 1.1: Confirm Premium Expansion Details
1.  **AI Action**: Confirm with the USER that you have received (or request if missing) the complete list of new cards for the Premium expansion.
2.  **Required Information for Each Card**:
    *   Card Name (e.g., Bishop, Jester, Dowager Queen, Constable, etc.)
    *   Card Value/Rank (integer)
    *   Quantity of this card in the deck.
    *   Detailed Effect Description: Targeting rules (no target, target self, target other, target any), conditions for the effect, outcome of the effect (player elimination, view card, force discard, gain token, protection, etc.), any new game mechanics introduced.
3.  **AI Action**: Confirm total new card types, total cards in the Premium deck, and any rule modifications based on player count.
4.  **AI Action**: Store this information carefully for reference in subsequent tasks.

## Phase 2: Domain Analysis (Minimizing Domain Changes)

**Goal**: Identify if any changes are absolutely necessary in `CardGame.Domain`. Prefer solutions within `CardGame.Decks.Premium` if possible.

### Task 2.1: Review `CardGame.Domain.Interfaces.IGameOperations`
1.  **AI Action**: Locate and view `CardGame.Domain.Interfaces.IGameOperations.cs`.
2.  **AI Action**: For each Premium card effect (from Task 1.1), determine if existing methods in `IGameOperations` are sufficient.
3.  **AI Action**: If new operations are *unavoidably* needed, list their proposed signatures, purpose, and why they are essential for the domain. (e.g., `void AwardTemporaryAffectionToken(PlayerId playerId);`).
4.  **AI Action**: Present findings for `IGameOperations` to the USER for approval before proposing any file changes.

### Task 2.2: Review `CardGame.Domain.Game.Player`
1.  **AI Action**: Locate and view `CardGame.Domain.Game.Player.cs`.
2.  **AI Action**: Determine if the [Player](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:48:4-53:34) class needs new properties or methods to support Premium mechanics (e.g., a new status for a unique protection type).
3.  **AI Action**: If new members are *unavoidably* needed, list their proposed definitions and purpose.
4.  **AI Action**: Present findings for [Player](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:48:4-53:34) to the USER for approval.

### Task 2.3: Review `CardGame.Domain.GameLogEventType`
1.  **AI Action**: Locate and view `CardGame.Domain.GameLogEventType.cs` (or its source, likely an EnumLike generated class).
2.  **AI Action**: Identify if new log event types are needed for Premium card actions.
3.  **AI Action**: If new types are needed, list their proposed names.
4.  **AI Action**: Present findings for `GameLogEventType` to the USER for approval.

### Task 2.4: Domain Impact Summary & USER Approval
1.  **AI Action**: Summarize all *proposed and justified* changes (if any) to `CardGame.Domain` from Tasks 2.1-2.3.
2.  **AI Action**: Await USER approval before making any changes to `CardGame.Domain` files. If no changes are needed, state this and await confirmation to proceed.

## Phase 3: Setting Up `CardGame.Decks.Premium` Project

**Assumption**: `CardGame.Decks.Premium` project exists but is minimal. It does NOT depend on `CardGame.Decks.BaseGame`.

### Task 3.1: Create `PremiumCardRank.cs`
1.  **AI Action**: Verify `CardGame.Decks.BaseGame.CardRank.cs` exists. View its content to understand its structure (EnumLike using `GeneratorAttributes`).
2.  **AI Action**: Create a new file: `C:\Users\squir\source\repos\CardGame\CardGame.Decks.Premium\PremiumCardRank.cs`.
3.  **AI Action**: Populate `PremiumCardRank.cs` by adapting the structure from `BaseGame.CardRank.cs`. Define all Premium card ranks (from Task 1.1) with their names and integer values. Ensure `GeneratorAttributes.EnumLike` and `GeneratedEnumValue` attributes are used correctly.
    *Example structure:*
    ```csharp
    using GeneratorAttributes;
    namespace CardGame.Decks.Premium;
    [EnumLike]
    public sealed partial class PremiumCardRank 
    {
        [GeneratedEnumValue] private static readonly int _bishop = 9; // Value example
        // ... other premium cards
    }
    ```
4.  **AI Action**: Present the full content of `PremiumCardRank.cs` for USER review.

### Task 3.2: Create `PremiumDeckProvider.cs` (Initial Structure)
1.  **AI Action**: Verify `CardGame.Decks.BaseGame.DefaultDeckProvider.cs` exists. View its content to understand its structure (inherits `BaseDeckProvider`, implements `ICardEffectExecutor` implicitly via `BaseDeckProvider`, overrides for deck properties, card quantities, appearance, and has `ExecuteCardEffect` and helper methods).
2.  **AI Action**: Create a new file: `C:\Users\squir\source\repos\CardGame\CardGame.Decks.Premium\PremiumDeckProvider.cs`.
3.  **AI Action**: Populate `PremiumDeckProvider.cs` by adapting the structure from `BaseGame.DefaultDeckProvider.cs`.
    *   Class `PremiumDeckProvider` should inherit `CardGame.Domain.Providers.BaseDeckProvider`.
    *   Add necessary `using` statements (e.g., `CardGame.Domain`, `CardGame.Domain.Game`, `CardGame.Domain.Interfaces`, `CardGame.Domain.Providers`, `System.Collections.Generic`, `System`).
4.  **AI Action**: Present the initial structure of `PremiumDeckProvider.cs` (class definition, constructor if any, empty method stubs for overrides) for USER review.

### Task 3.3: Implement Properties in `PremiumDeckProvider.cs`
1.  **AI Action**: In `PremiumDeckProvider.cs`, implement the following overrides:
    *   `public override Guid DeckId => new("NEW_UNIQUE_GUID_FOR_PREMIUM_DECK");` (Generate a new, valid GUID).
    *   `public override string DisplayName => "Love Letter Premium Edition";` (Or other suitable name).
    *   `public override string Description => "The Premium expansion for Love Letter with new characters and effects.";`
    *   `protected override string ThemeName => "premium";`
    *   `protected override string DeckBackAppearanceId => "assets/decks/premium/back.webp";` (Confirm asset path convention).
2.  **AI Action**: Present the updated `PremiumDeckProvider.cs` section for USER review.

### Task 3.4: Implement [GetCardQuantities()](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:26:4-39:5) in `PremiumDeckProvider.cs`
1.  **AI Action**: In `PremiumDeckProvider.cs`, implement the `protected override IEnumerable<CardQuantity> GetCardQuantities()` method.
2.  **AI Action**: This method should return a `List<CardQuantity>` based on the card list and quantities from Task 1.1, using `PremiumCardRank.YourCard.Value` for ranks.
    *Example: `new CardQuantity(PremiumCardRank.Bishop.Value, 1)`*
3.  **AI Action**: Present the [GetCardQuantities()](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:26:4-39:5) method for USER review.

### Task 3.5: Implement [GetCardAppearanceId()](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:41:4-46:5) in `PremiumDeckProvider.cs`
1.  **AI Action**: In `PremiumDeckProvider.cs`, implement `protected override string GetCardAppearanceId(int rankValue, int index)`.
2.  **AI Action**: Use `PremiumCardRank.FromValue(rankValue)` to get the card rank object. Construct the path: `$"assets/decks/premium/{cardRank.Name.ToLowerInvariant()}.webp"` (Confirm asset path convention and naming, e.g., `bishop.webp`).
3.  **AI Action**: Present the [GetCardAppearanceId()](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:41:4-46:5) method for USER review.

### Task 3.6: Implement `ExecuteCardEffect()` (Skeleton) and Helper Stubs in `PremiumDeckProvider.cs`
1.  **AI Action**: In `PremiumDeckProvider.cs`, override `public override void ExecuteCardEffect(IGameOperations game, Player actingPlayer, Card playedCard, Player? targetPlayer, PremiumCardRank? guessedCardRank, int? guessedCardValue = null)` (Note: `guessedCardRank` might need to be `PremiumCardRank?` or adapt from `BaseDeckProvider`'s signature if it uses a generic or base [CardRank](cci:2://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/CardRank.cs:4:0-46:1)). **AI must verify the correct signature from `BaseDeckProvider`**.
2.  The method should have a `switch` statement based on `PremiumCardRank.FromValue(playedCard.Rank.Value)`.
3.  For each card in `PremiumCardRank` (from Task 1.1), add a `case` in the switch that calls a private helper method stub, e.g., `ExecuteBishopEffect(game, actingPlayer, playedCard, targetPlayer);`.
4.  Create private void method stubs for each effect, e.g., `private void ExecuteBishopEffect(...) { /* TODO */ }`.
5.  **AI Action**: Present the `ExecuteCardEffect` method and all helper method stubs for USER review.

### Task 3.7: Update [CardGame.Decks.Premium.csproj](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.Premium/CardGame.Decks.Premium.csproj:0:0-0:0)
1.  **AI Action**: View the content of [C:\Users\squir\source\repos\CardGame\CardGame.Decks.Premium\CardGame.Decks.Premium.csproj](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.Premium/CardGame.Decks.Premium.csproj:0:0-0:0).
2.  **AI Action**: Ensure it references `CardGame.Domain` (`<ProjectReference Include="..\CardGame.Domain\CardGame.Domain.csproj" />`) and any other necessary packages (e.g., `GeneratorAttributes` if not transitively referenced).
3.  **AI Action**: Remove any placeholder files like [Class1.cs](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.Premium/Class1.cs:0:0-0:0) if they exist and are no longer needed.
4.  **AI Action**: Present proposed changes to the [.csproj](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Web/CardGame.Web.csproj:0:0-0:0) file for USER review and apply them.

### Task 3.8: Create [README.md](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Web/README.md:0:0-0:0) for `CardGame.Decks.Premium`
1.  **AI Action**: Create/Update `C:\Users\squir\source\repos\CardGame\CardGame.Decks.Premium\README.md`.
2.  **AI Action**: Include: Purpose of the expansion, list of new cards (from Task 1.1), brief description of their effects, and any unique rules. State that it's a self-contained module.
3.  **AI Action**: Present the [README.md](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Web/README.md:0:0-0:0) content for USER review.

## Phase 4: Implementing Card Effects (Iterative)

**For each Premium card identified in Task 1.1, perform the following sub-tasks:**

### Task 4.X: Implement Effect for [Specific Premium Card Name]
1.  **AI Action**: Focus on the `private void Execute[SpecificPremiumCardName]Effect(...)` method in `PremiumDeckProvider.cs`.
2.  **AI Action**: Implement the card's logic as defined in Task 1.1.
    *   Use `game` (IGameOperations) to interact with game state (e.g., `EliminatePlayer`, `AddLogEntry`, `DrawCard`, `ProtectPlayer`, `ViewCardInHand`). **Verify all method names and signatures on `IGameOperations` before use.**
    *   Access `actingPlayer` and `targetPlayer` properties as needed. **Verify all property names on [Player](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:48:4-53:34) before use.**
    *   Handle targeting rules, conditions, and outcomes.
    *   Ensure interactions with protective effects (e.g., Handmaid-like effects, or new ones from Premium) are considered.
3.  **AI Action**: Add comprehensive `GameLogEntry` calls for all significant actions, decisions, and outcomes of the card effect. Use appropriate `GameLogEventType` values (from Task 2.3, or existing ones if suitable).
4.  **AI Action**: Present the completed `Execute[SpecificPremiumCardName]Effect(...)` method and its call site in `ExecuteCardEffect` for USER review.
5.  **AI Action**: Await USER approval before moving to the next card.

## Phase 5: Unit Testing (Iterative)

**Assumption**: `CardGame.Decks.PremiumTests` project exists.

### Task 5.1: Setup `PremiumDeckProviderTests.cs`
1.  **AI Action**: Create `C:\Users\squir\source\repos\CardGame\CardGame.Decks.PremiumTests\PremiumDeckProviderTests.cs`.
2.  **AI Action**: Add NUnit test fixture attributes (`[TestFixture]`). Include `SetUp` method with mocks for `IGameOperations`, [Player](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:48:4-53:34), etc., using Moq. Include `FakeLoggerFactory` if logging is tested.
3.  **AI Action**: Present the basic test class structure for USER review.

### Task 5.2: Test `PremiumDeckProvider` Properties and Deck Composition
1.  **AI Action**: In `PremiumDeckProviderTests.cs`, write tests to:
    *   Verify `DeckId`, `DisplayName`, `Description`, `ThemeName`, `DeckBackAppearanceId` are correct.
    *   Verify [GetCardQuantities()](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:26:4-39:5) returns the correct card types and counts.
    *   Verify `GetCardDeck()` (from `BaseDeckProvider`) returns a deck with the correct total number of cards and composition based on [GetCardQuantities()](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:26:4-39:5).
    *   Verify [GetCardAppearanceId()](cci:1://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.BaseGame/DefaultDeckProvider.cs:41:4-46:5) returns expected paths for each rank.
2.  **AI Action**: Present these test methods for USER review.

**For each Premium card effect implemented in Phase 4, perform the following sub-tasks:**

### Task 5.X: Create Unit Tests for [Specific Premium Card Name] Effect
1.  **AI Action**: In `PremiumDeckProviderTests.cs`, add test methods specifically for `Execute[SpecificPremiumCardName]Effect`.
2.  **AI Action**: For each test case:
    *   Set up mock `IGameOperations`, `actingPlayer`, `targetPlayer` (if applicable), and `playedCard` to reflect the scenario being tested.
    *   Call the `ExecuteCardEffect` method on an instance of `PremiumDeckProvider`.
    *   Assert that the correct `IGameOperations` methods were called (e.g., `game.Verify(g => g.EliminatePlayer(...), Times.Once())`).
    *   Assert player states are modified as expected (e.g., `targetPlayer.IsEliminated` is true).
    *   Assert correct `GameLogEntry` events were added if applicable.
3.  **AI Action**: Cover various scenarios: successful effect, fizzle due to protection, invalid target, specific conditions met/not met.
4.  **AI Action**: Present the test methods for this card for USER review.
5.  **AI Action**: Await USER approval before moving to the next card's tests.

### Task 5.Y: Update [CardGame.Decks.PremiumTests.csproj](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.PremiumTests/CardGame.Decks.PremiumTests.csproj:0:0-0:0)
1.  **AI Action**: View [CardGame.Decks.PremiumTests.csproj](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Decks.PremiumTests/CardGame.Decks.PremiumTests.csproj:0:0-0:0).
2.  **AI Action**: Ensure it references `CardGame.Decks.Premium`, `CardGame.Domain`, NUnit, Moq, FluentAssertions, and other testing necessities.
3.  **AI Action**: Present proposed changes for USER review and apply them.

## Phase 6: Integration

### Task 6.1: Register `PremiumDeckProvider` in Dependency Injection
1.  **AI Action**: Identify where `IDeckProvider` implementations are registered for DI. This is likely in `CardGame.Infrastructure` or a similar project, possibly involving an `IDeckProviderRegistrar` or direct registration in `Startup.cs`/[Program.cs](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Web/Program.cs:0:0-0:0) extension methods.
2.  **AI Action**: **Verify the exact file and registration mechanism by searching the codebase.**
3.  **AI Action**: Add code to register `CardGame.Decks.Premium.PremiumDeckProvider` with the DI container.
4.  **AI Action**: Present the proposed changes for USER review and apply them.

## Phase 7: Documentation Finalization

### Task 7.1: Update Main Project [README.md](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Web/README.md:0:0-0:0)
1.  **AI Action**: Locate the main solution [README.md](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Web/README.md:0:0-0:0) (likely [C:\Users\squir\source\repos\CardGame\README.md](cci:7://file:///Users/squir/source/repos/CardGame/README.md:0:0-0:0)).
2.  **AI Action**: Add a brief mention of the new `CardGame.Decks.Premium` expansion and its availability.
3.  **AI Action**: Present the change for USER review and apply it.

### Task 7.2: Final Review of `CardGame.Decks.Premium/README.md`
1.  **AI Action**: Review the [README.md](cci:7://file:///C:/Users/squir/source/repos/CardGame/CardGame.Web/README.md:0:0-0:0) created in Task 3.8. Ensure it's comprehensive and accurately reflects the implemented Premium expansion.
2.  **AI Action**: Make any necessary updates based on the final implementation.
3.  **AI Action**: Present the final `CardGame.Decks.Premium/README.md` for USER review.

## Phase 8: Final Review and Cleanup

### Task 8.1: Code Review and Cleanup
1.  **AI Action**: Scan all newly generated code in `CardGame.Decks.Premium` and `CardGame.Decks.PremiumTests` for clarity, correctness, adherence to project patterns, and any `TODO` comments.
2.  **AI Action**: List any concerns or areas for potential refinement for the USER.

**AI: Await USER confirmation to begin Task 1.1.**