using CardGame.Domain.Providers;
using CardGame.Domain.Types;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using CardGame.Domain.Game;
using CardGame.Domain.Game.GameException;
using Moq;
using CardGame.Domain.Interfaces;
using CardRank = CardGame.Domain.BaseGame.CardRank;

namespace CardGame.Domain.Tests.Providers
{
    [TestFixture]
    public class CthulhuDeckProviderTests // Changed class name
    {
        private CthulhuDeckProvider _provider; // Changed provider type
        private ILogger<Player> _playerLogger;
        private Mock<IGameOperations> _mockGameOperations;

        [SetUp]
        public void SetUp()
        {
            _provider = new CthulhuDeckProvider(); // Changed provider instantiation
            _playerLogger = NullLogger<Player>.Instance;
            _mockGameOperations = new Mock<IGameOperations>();
        }

        // --- Deck Composition Tests --- 
        [Test]
        public void GetDeck_ShouldReturnCorrectTotalNumberOfCards()
        {
            // Act
            var cards = _provider.GetDeck().Cards;

            // Assert
            Assert.That(cards.Count(), Is.EqualTo(16), "Deck should contain 16 cards.");
        }

        [TestCase(1, 5, "Guard")] // Thematic names (Investigator, etc.) are just for the message here
        [TestCase(2, 2, "Priest")]
        [TestCase(3, 2, "Baron")]
        [TestCase(4, 2, "Handmaid")]
        [TestCase(5, 2, "Prince")]
        [TestCase(6, 1, "King")]
        [TestCase(7, 1, "Countess")]
        [TestCase(8, 1, "Princess")]
        public void GetDeck_ShouldReturnCorrectCountOfEachCardType(int cardRank, int expectedCount, string cardNameForMessage)
        {
            // Arrange
            var cardType = CardRank.FromValue(cardRank);
            
            // Act
            var cards = _provider.GetDeck().Cards;
            var count = cards.Count(card => card.Rank == cardType);

            // Assert
            Assert.That(count, Is.EqualTo(expectedCount), $"Deck should contain {expectedCount} {cardNameForMessage} cards.");
        }
        
        [Test]
        public void GetDeck_AllCardsShouldHaveNonNullAndNotEmptyAppearanceIds()
        {
            // Act
            var cards = _provider.GetDeck().Cards;

            // Assert
            Assert.That(cards.All(c => !string.IsNullOrEmpty(c.AppearanceId)), Is.True, "All cards should have a non-null and non-empty AppearanceId.");
        }

        [Test]
        public void DeckId_ShouldReturnCorrectGuid()
        {
            // Assert
            Assert.That(_provider.DeckId, Is.EqualTo(new System.Guid("00000000-0000-0000-0000-000000000002")), "DeckId should be the Cthulhu deck GUID."); // Changed DeckId
        }

        // --- ExecuteCardEffect Tests ---
        // Note: These tests will also cover the validation logic previously in CanPlayCard.
        // Invalid plays should result in InvalidMoveException.

        [Test]
        public void ExecuteCardEffect_Guard_NoTargetPlayer_ThrowsInvalidMoveException()
        {
            // Arrange
            var guardCard = new Card("guard_1.webp", CardRank.Guard);
            var playerHand = Hand.Load(new List<Card> { guardCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, guardCard, null, CardRank.Priest));
            Assert.That(ex.Message, Is.EqualTo("Guard requires a target player."));
        }

        [Test]
        public void ExecuteCardEffect_Guard_NoGuessedCardType_ThrowsInvalidMoveException()
        {
            // Arrange
            var guardCard = new Card("guard_1.webp", CardRank.Guard);
            var playerHand = Hand.Load(new List<Card> { guardCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            var targetPlayer = Player.Load(Guid.NewGuid(), "P2", PlayerStatus.Active, Hand.Load(new List<Card> { new Card("priest.webp", CardRank.Priest) }), new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, guardCard, targetPlayer, null));
            Assert.That(ex.Message, Is.EqualTo("Guard requires a guessed card type."));
        }

        [Test]
        public void ExecuteCardEffect_Guard_GuessingGuard_ThrowsInvalidMoveException()
        {
            // Arrange
            var guardCard = new Card("guard_1.webp", CardRank.Guard);
            var playerHand = Hand.Load(new List<Card> { guardCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            var targetPlayer = Player.Load(Guid.NewGuid(), "P2", PlayerStatus.Active, Hand.Load(new List<Card> { new Card("priest.webp", CardRank.Priest) }), new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, guardCard, targetPlayer, CardRank.Guard));
            Assert.That(ex.Message, Is.EqualTo("Cannot guess Guard with a Guard."));
        }

        [Test]
        public void ExecuteCardEffect_Guard_TargetingSelf_ThrowsInvalidMoveException()
        {
            // Arrange
            var guardCard = new Card("guard_1.webp", CardRank.Guard);
            var playerHand = Hand.Load(new List<Card> { guardCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, guardCard, actingPlayer, CardRank.Priest));
            Assert.That(ex.Message, Is.EqualTo("Guard cannot target self."));
        }

        // Countess Rule Tests for ExecuteCardEffect
        [Test]
        public void ExecuteCardEffect_PlayerHoldsCountessAndKing_PlayingKing_ThrowsInvalidMoveException()
        {
            // Arrange
            var countessCard = new Card("countess.webp", CardRank.Countess);
            var kingCard = new Card("king.webp", CardRank.King);
            var playerHand = Hand.Load(new List<Card> { countessCard, kingCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            var targetPlayer = Player.Load(Guid.NewGuid(), "P2", PlayerStatus.Active, Hand.Load(new List<Card> { new Card("priest.webp", CardRank.Priest) }), new List<CardRank>(), 0, false, _playerLogger); // King needs a target

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, kingCard, targetPlayer, null));
            Assert.That(ex.Message, Is.EqualTo("Cannot play King when holding the Countess (and King/Prince). You must play the Countess."));
        }

        [Test]
        public void ExecuteCardEffect_PlayerHoldsCountessAndPrince_PlayingPrince_ThrowsInvalidMoveException()
        {
            // Arrange
            var countessCard = new Card("countess.webp", CardRank.Countess);
            var princeCard = new Card("prince.webp", CardRank.Prince);
            var playerHand = Hand.Load(new List<Card> { countessCard, princeCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            var targetPlayer = Player.Load(Guid.NewGuid(), "P2", PlayerStatus.Active, Hand.Load(new List<Card> { new Card("priest.webp", CardRank.Priest) }), new List<CardRank>(), 0, false, _playerLogger); // Prince can target others

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, princeCard, targetPlayer, null));
            Assert.That(ex.Message, Is.EqualTo("Cannot play Prince when holding the Countess (and King/Prince). You must play the Countess."));
        }

        // --- Priest Tests ---
        [Test]
        public void ExecuteCardEffect_Priest_NoTargetPlayer_ThrowsInvalidMoveException()
        {
            // Arrange
            var priestCard = new Card("priest.webp", CardRank.Priest);
            var playerHand = Hand.Load(new List<Card> { priestCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, priestCard, null, null));
            Assert.That(ex.Message, Is.EqualTo("Priest requires a target player."));
        }

        [Test]
        public void ExecuteCardEffect_Priest_TargetingSelf_ThrowsInvalidMoveException()
        {
            // Arrange
            var priestCard = new Card("priest.webp", CardRank.Priest);
            var playerHand = Hand.Load(new List<Card> { priestCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, priestCard, actingPlayer, null));
            Assert.That(ex.Message, Is.EqualTo("Priest cannot target self."));
        }

        // --- Baron Tests ---
        [Test]
        public void ExecuteCardEffect_Baron_NoTargetPlayer_ThrowsInvalidMoveException()
        {
            // Arrange
            var baronCard = new Card("baron.webp", CardRank.Baron);
            var playerHand = Hand.Load(new List<Card> { baronCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, baronCard, null, null));
            Assert.That(ex.Message, Is.EqualTo("Baron requires a target player."));
        }

        [Test]
        public void ExecuteCardEffect_Baron_TargetingSelf_ThrowsInvalidMoveException()
        {
            // Arrange
            var baronCard = new Card("baron.webp", CardRank.Baron);
            var playerHand = Hand.Load(new List<Card> { baronCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, baronCard, actingPlayer, null));
            Assert.That(ex.Message, Is.EqualTo("Baron cannot target self."));
        }

        // --- King Tests ---
        [Test]
        public void ExecuteCardEffect_King_NoTargetPlayer_ThrowsInvalidMoveException()
        {
            // Arrange
            var kingCard = new Card("king.webp", CardRank.King);
            var playerHand = Hand.Load(new List<Card> { kingCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, kingCard, null, null));
            Assert.That(ex.Message, Is.EqualTo("King requires a target player."));
        }

        [Test]
        public void ExecuteCardEffect_King_TargetingSelf_ThrowsInvalidMoveException()
        {
            // Arrange
            var kingCard = new Card("king.webp", CardRank.King);
            var playerHand = Hand.Load(new List<Card> { kingCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, kingCard, actingPlayer, null));
            Assert.That(ex.Message, Is.EqualTo("King cannot target self."));
        }

        // --- Prince Tests ---
        [Test]
        public void ExecuteCardEffect_Prince_NoTargetPlayer_ThrowsInvalidMoveException()
        {
            // Arrange
            var princeCard = new Card("prince.webp", CardRank.Prince);
            var playerHand = Hand.Load(new List<Card> { princeCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);

            // Act & Assert
            var ex = Assert.Throws<InvalidMoveException>(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, princeCard, null, null));
            Assert.That(ex.Message, Is.EqualTo("Prince requires a target player."));
        }

        [Test]
        public void ExecuteCardEffect_Prince_TargetingSelf_DoesNotThrow()
        {
            // Arrange
            var princeCard = new Card("prince.webp", CardRank.Prince);
            var playerHand = Hand.Load(new List<Card> { princeCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            _mockGameOperations.Setup(g => g.AddLogEntry(It.IsAny<GameLogEntry>()));
            _mockGameOperations.Setup(g => g.DrawCardForPlayer(actingPlayer.Id)); // Prince effect might draw a card

            // Act & Assert
            Assert.DoesNotThrow(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, princeCard, actingPlayer, null), // Target self
                "Prince targeting self should not throw an InvalidMoveException due to targeting rules.");
        }

        // --- Countess Valid Play Scenarios ---
        [Test]
        public void ExecuteCardEffect_PlayerHoldsCountessAndKing_PlayingCountess_DoesNotThrow()
        {
            // Arrange
            var countessCard = new Card("countess.webp", CardRank.Countess);
            var kingCard = new Card("king.webp", CardRank.King);
            var playerHand = Hand.Load(new List<Card> { countessCard, kingCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            _mockGameOperations.Setup(g => g.AddLogEntry(It.IsAny<GameLogEntry>()));

            // Act & Assert
            Assert.DoesNotThrow(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, countessCard, null, null),
                "Playing Countess when holding Countess and King should be valid.");
        }

        [Test]
        public void ExecuteCardEffect_PlayerHoldsCountessAndPrince_PlayingCountess_DoesNotThrow()
        {
            // Arrange
            var countessCard = new Card("countess.webp", CardRank.Countess);
            var princeCard = new Card("prince.webp", CardRank.Prince);
            var playerHand = Hand.Load(new List<Card> { countessCard, princeCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            _mockGameOperations.Setup(g => g.AddLogEntry(It.IsAny<GameLogEntry>()));

            // Act & Assert
            Assert.DoesNotThrow(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, countessCard, null, null),
                "Playing Countess when holding Countess and Prince should be valid.");
        }

        [Test]
        public void ExecuteCardEffect_PlayerHoldsCountessNoKingOrPrince_PlayingOtherCard_DoesNotThrow()
        {
            // Arrange
            var countessCard = new Card("countess.webp", CardRank.Countess);
            var guardCard = new Card("guard.webp", CardRank.Guard);
            var playerHand = Hand.Load(new List<Card> { countessCard, guardCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            var targetPlayer = Player.Load(Guid.NewGuid(), "P2", PlayerStatus.Active, Hand.Load(new List<Card> { new Card("priest.webp", CardRank.Priest) }), new List<CardRank>(), 0, false, _playerLogger);
            _mockGameOperations.Setup(g => g.AddLogEntry(It.IsAny<GameLogEntry>()));
            // Mock for Guard effect if it proceeds
            _mockGameOperations.Setup(g => g.GetPlayer(targetPlayer.Id)).Returns(targetPlayer);
            var targetCardInHand = new Card("baron.webp", CardRank.Baron);
            targetPlayer.Hand.Add(targetCardInHand); // Ensure target has a card for Guard to potentially interact with

            // Act & Assert
            Assert.DoesNotThrow(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, guardCard, targetPlayer, CardRank.Baron), // Playing Guard
                "Playing another card (Guard) when holding Countess but no King/Prince should be valid.");
        }

        [Test]
        public void ExecuteCardEffect_PlayerPlaysCountess_NoKingOrPrinceInHand_DoesNotThrow()
        {
            // Arrange
            var countessCard = new Card("countess.webp", CardRank.Countess);
            var guardCard = new Card("guard.webp", CardRank.Guard); // Another card, not King or Prince
            var playerHand = Hand.Load(new List<Card> { countessCard, guardCard });
            var actingPlayer = Player.Load(Guid.NewGuid(), "P1", PlayerStatus.Active, playerHand, new List<CardRank>(), 0, false, _playerLogger);
            _mockGameOperations.Setup(g => g.AddLogEntry(It.IsAny<GameLogEntry>()));

            // Act & Assert
            Assert.DoesNotThrow(() => 
                _provider.ExecuteCardEffect(_mockGameOperations.Object, actingPlayer, countessCard, null, null), // Playing Countess
                "Playing Countess when not holding King or Prince should be valid.");
        }
    }
}
