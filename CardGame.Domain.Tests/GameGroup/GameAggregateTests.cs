using CardGame.Domain.Game.Event;
using CardGame.Domain.Tests.TestDoubles;
using CardGame.Domain.Types;
using FluentAssertions;

namespace CardGame.Domain.Tests.GameGroup;

    [TestFixture] // NUnit attribute to mark a class containing tests
    public class GameAggregateTests
    {
        // No SetUp needed for this specific test

        [Test] // NUnit attribute marks this method as a test case (replaced [Fact])
        public void PlayCard_WhenPlayer1PlaysGuardSuccessfully_ShouldUpdateStateAndRaiseEvents()
        {
            // ARRANGE

            // 1. Create the game with two players
            var playerNames = new List<string> { "Alice", "Bob" };
            var game = Game.Game.CreateNewGame(playerNames, tokensToWin: 4);
            var player1 = game.Players.First(p => p.Name == "Alice");
            var player2 = game.Players.First(p => p.Name == "Bob");

            // 2. Use the deterministic randomizer for predictable deck order
            var deterministicRandomizer = new NonShufflingRandomizer();

            // 3. Start the first round using the non-shuffling randomizer
            game.StartNewRound(randomizer: deterministicRandomizer);

            // 4. Get Player 1's actual hand cards after dealing and drawing
            // Because the deck isn't shuffled, we know the types, but the Guids are random.
            // Expected Types: Guard, Guard (based on previous prediction logic)
            var player1HandCards = player1.Hand.GetCards().ToList();
            player1HandCards.Should().HaveCount(2);
            player1HandCards.Should().AllSatisfy(c => c.Type.Should().Be(CardType.Guard));

            // Choose the first card instance from the hand to play
            var cardToPlayInstance = player1HandCards[0];
            // The other card is the one to keep
            var cardToKeepInstance = player1HandCards[1];

            // Define the target and guess for the Guard play
            var targetPlayerId = player2.Id;
            // Guess Princess - guaranteed wrong as P2 has a Guard
            var guessedCardType = CardType.Princess;

            // 5. Clear events raised during setup
            game.ClearDomainEvents();

            // ACT

            // Player 1 plays the specific Guard instance they were dealt first
            game.PlayCard(player1.Id, cardToPlayInstance, targetPlayerId, guessedCardType);


            // ASSERT

            // 1. Turn should advance to Player 2
            game.CurrentTurnPlayerId.Should().Be(player2.Id);

            // 2. Player 1's hand should contain only the card they kept
            player1.Hand.Count.Should().Be(1);
            player1.Hand.GetHeldCard().Should().Be(cardToKeepInstance); // Check specific instance kept

            // 3. Player 1's played cards should include the type played
            player1.PlayedCards.Should().Contain(cardToPlayInstance.Type);

            // 4. Discard pile should contain the specific card instance played
            game.DiscardPile.Should().Contain(cardToPlayInstance);
            game.DiscardPile.Last().Should().Be(cardToPlayInstance);

            // 5. Player 2 should still be active
            player2.Status.Should().Be(PlayerStatus.Active);
            // P2 was dealt Guard(2), drew Guard(0). Should have 1 card after P1 turn ends, P2 turn starts+draws.
            // Let's check the type and count after P2's draw (which happens in AdvanceTurn called by PlayCard)
            player2.Hand.Count.Should().Be(2); // P2 drew at start of their turn
            player2.Hand.GetHeldCard()?.Type.Should().Be(CardType.Guard); // Check the type


            // 6. Correct domain events should have been raised by the PlayCard action
            var events = game.DomainEvents.ToList();

            // Event 1: PlayerPlayedCard
            events.Should().ContainSingle(e => e is PlayerPlayedCard)
                  .Which.Should().BeEquivalentTo(new PlayerPlayedCard(
                      game.Id,
                      player1.Id,
                      cardToPlayInstance.Type, // Event contains the Type played
                      targetPlayerId,
                      guessedCardType
                  ), options => options.Excluding(e => e.EventId).Excluding(e => e.OccurredOn).Excluding(e => e.CorrelationId));

            // Event 2: GuardGuessResult (Guess was Princess, target has Guard -> incorrect)
            events.Should().ContainSingle(e => e is GuardGuessResult)
                  .Which.Should().BeEquivalentTo(new GuardGuessResult(
                      game.Id,
                      player1.Id,
                      targetPlayerId,
                      guessedCardType, // Princess
                      false // Guess was incorrect
                  ), options => options.Excluding(e => e.EventId).Excluding(e => e.OccurredOn).Excluding(e => e.CorrelationId));


            // Event 3: TurnStarted (for Player 2)
             events.Should().ContainSingle(e => e is TurnStarted)
                  .Which.Should().BeEquivalentTo(new TurnStarted(
                      game.Id,
                      player2.Id, // Player 2's turn started
                      game.RoundNumber
                  ), options => options.Excluding(e => e.EventId).Excluding(e => e.OccurredOn).Excluding(e => e.CorrelationId));

             // Event 4: PlayerDrewCard (for Player 2)
             events.Should().ContainSingle(e => e is PlayerDrewCard && ((PlayerDrewCard)e).PlayerId == player2.Id);

             // Event 5: DeckChanged (for Player 2's draw)
             events.Should().ContainSingle(e => e is DeckChanged);
             // Deck started with 16. SetAside=1, DealP1=1, DealP2=1, P1Draw=1, P1Play=0, P2Draw=1 -> 16-5 = 11 left
             events.OfType<DeckChanged>().Single().CardsRemaining.Should().Be(11);


             // Check total number of events raised by PlayCard action
             events.Should().HaveCount(5); // PlayerPlayedCard, GuardGuessResult, TurnStarted, PlayerDrewCard, DeckChanged
        }
    }