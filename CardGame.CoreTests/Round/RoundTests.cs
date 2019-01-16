using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Card;
using CardGame.Core.Hand;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CardGame.CoreTests.Round
{
    [TestClass]
    public class RoundTests
    {
        private IEnumerable<Guid> _deck;

        private IEnumerable<Guid> _players;
        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            mockRepository.VerifyAll();
        }

        private Core.Round.Round CreateRound(IDictionary<Guid, Hand> playerHands,
            IEnumerable<Guid> deck,
            IEnumerable<Guid> playerOrder = null,
            ushort nextRoundId = 0,
            IEnumerable<Guid> protectedPlayers = null,
            IEnumerable<Guid> discarded = null,
            IEnumerable<Guid> setAsideCards = null,
            ushort turnId = 0)
        {
            protectedPlayers = protectedPlayers ?? new Guid[] { };
            discarded = discarded ?? new Guid[] { };
            setAsideCards = setAsideCards ?? new Guid[] { };
            var id = Guid.Parse("89e99225-35c0-4a63-a62e-f61aa65a1451");
            return new Core.Round.Round(
                playerOrder ?? playerHands.Keys,
                playerHands,
                deck,
                id,
                nextRoundId,
                protectedPlayers,
                discarded,
                setAsideCards,
                turnId);
        }

        private Core.Round.Round CreateRound(IDictionary<Guid, Guid> playerHands,
            IEnumerable<Guid> deck,
            IEnumerable<Guid> playerOrder = null,
            ushort nextRoundId = 0,
            IEnumerable<Guid> protectedPlayers = null,
            IEnumerable<Guid> discarded = null,
            IEnumerable<Guid> setAsideCards = null,
            ushort currentTurnId = 0)
        {
            return CreateRound(
                playerHands.ToDictionary(kvp => kvp.Key, kvp => new Hand(kvp.Value)),
                deck,
                playerOrder,
                nextRoundId,
                protectedPlayers,
                discarded,
                setAsideCards,
                currentTurnId);
        }

        //[TestMethod]
        //public void DiscardAndDraw_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);
        //    Guid targetId = TODO;

        //    // Act
        //    unitUnderTest.DiscardAndDraw(
        //        targetId);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void GetCurrentPlayerHand_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);

        //    // Act
        //    var result = unitUnderTest.GetPlayerHand();

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void RevealHand_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);
        //    Guid targetPlayerId = TODO;

        //    // Act
        //    var result = unitUnderTest.RevealHand(
        //        targetPlayerId);

        //    // Assert
        //    Assert.Fail();
        //}

        [TestMethod]
        public void PlayKing_TargetNotProtected_TradesHands()
        {
            // Arrange
            var sourcePlayerId = Guid.Empty;
            var card1 = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var card2 = Guid.Parse("15b62fab-19cc-4029-b548-d00d25681c38");
            var cardId = Guid.Parse("418a5473-73c0-4ccb-8ab4-8cd2515d50ed");
            var targetPlayerId = Guid.Parse("54f2b615-efb9-49e3-b780-53dcd14777a5");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Hand>
                {
                    {sourcePlayerId, new Hand(card1, cardId)},
                    {targetPlayerId, new Hand(card2)}
                },
                new Guid[] { }
            );
            // Act
            var expected = card2;
            unitUnderTest.PlayKing(cardId,
                sourcePlayerId,
                targetPlayerId);
            var actual = unitUnderTest.GetPlayerHand(sourcePlayerId).Previous;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PlayKing_TargetProtected_DoesNotTradesHands()
        {
            // Arrange
            var sourcePlayerId = Guid.Empty;
            var card1 = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var card2 = Guid.Parse("15b62fab-19cc-4029-b548-d00d25681c38");
            var targetPlayerId = Guid.Parse("54f2b615-efb9-49e3-b780-53dcd14777a5");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Guid>
                {
                    {sourcePlayerId, card1},
                    {targetPlayerId, card2}
                },
                new Guid[] { },
                protectedPlayers: new[] {targetPlayerId}
            );
            // Act
            Guid? expected = card1;
            unitUnderTest.PlayKing(Guid.Parse("418a5473-73c0-4ccb-8ab4-8cd2515d50ed"),
                sourcePlayerId,
                targetPlayerId);
            var actual = unitUnderTest.GetPlayerHand(sourcePlayerId).Previous;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PlayPrincess_StateUnderTest_DoesNotRemain()
        {
            // Arrange
            var playerId = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var cardId = Guid.Parse("418a5473-73c0-4ccb-8ab4-8cd2515d50ed");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Guid>
                {
                    {playerId, cardId}
                },
                new Guid[] { });

            // Act
            unitUnderTest.PlayPrincess(cardId, playerId);

            // Assert
            Assert.IsFalse(unitUnderTest.RemainingPlayers.Contains(playerId));
        }

        [TestMethod]
        public void PlayPrincess_StateUnderTest_HandDiscarded()
        {
            // Arrange
            var playerId = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var cardId = Guid.Parse("15b62fab-19cc-4029-b548-d00d25681c38");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Guid>
                {
                    {playerId, cardId}
                },
                new Guid[] { });

            // Act
            unitUnderTest.PlayPrincess(cardId, playerId);

            // Assert
            Assert.IsTrue(unitUnderTest.Discarded.Contains(cardId));
        }

        [TestMethod]
        public void PlayHandmaid_StateUnderTest_IsProtected()
        {
            // Arrange
            var playerId = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var cardId = Guid.Parse("418a5473-73c0-4ccb-8ab4-8cd2515d50ed");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Hand>
                {
                    {playerId, new Hand(Guid.Parse("15b62fab-19cc-4029-b548-d00d25681c38"), cardId)}
                },
                new Guid[] { });

            // Act
            unitUnderTest.PlayHandmaid(cardId,
                playerId);

            // Assert
            Assert.IsTrue(unitUnderTest.ProtectedPlayers.Contains(playerId));
        }

        [TestMethod]
        public void GetWinningPlayerId_OnePlayerInRound_PlayerWins()
        {
            // Arrange
            var playerId = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Guid>
                {
                    {playerId, Guid.Parse("15b62fab-19cc-4029-b548-d00d25681c38")}
                },
                new Guid[] { });
            Core.Round.Round.GetCardValue getCardValue = cardId => CardValue.Guard;

            // Act
            var result = unitUnderTest.GetWinningPlayerId(
                getCardValue);

            // Assert
            var expected = playerId;
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetWinningPlayerId_TwoPlayers_HighValueWins()
        {
            // Arrange
            var playerId = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var playerCard = Guid.Parse("15b62fab-19cc-4029-b548-d00d25681c38");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Guid>
                {
                    {playerId, playerCard},
                    {Guid.Empty, Guid.Empty}
                },
                new Guid[] { });
            Core.Round.Round.GetCardValue getCardValue = cardId =>
                cardId == playerCard ? CardValue.Princess : CardValue.Guard;

            // Act
            var result = unitUnderTest.GetWinningPlayerId(
                getCardValue);

            // Assert
            var expected = playerId;
            Assert.AreEqual(expected, result);
        }

        //[TestMethod]
        //public void Start_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);
        //    GetCardValue getCardValue = TODO;

        //    // Act
        //    var result = unitUnderTest.Start(
        //        getCardValue);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void Cleanup_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);

        //    // Act
        //    unitUnderTest.Cleanup();

        //    // Assert
        //    Assert.Fail();
        //}

        [TestMethod]
        public void Discard_StateUnderTest_IsDiscarded()
        {
            // Arrange
            var playerId = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var playerCard = Guid.Parse("15b62fab-19cc-4029-b548-d00d25681c38");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Guid>
                {
                    {playerId, playerCard}
                },
                new Guid[] { });

            // Act
            unitUnderTest.Discard(playerId);

            // Assert
            Assert.IsTrue(unitUnderTest.Discarded.Contains(playerCard));
        }

        [TestMethod]
        public void Play_StateUnderTest_IsDiscarded()
        {
            // Arrange
            var playerId = Guid.Parse("bdc77745-76d4-4a52-88be-dd6efd144bea");
            var playerCard = Guid.Parse("15b62fab-19cc-4029-b548-d00d25681c38");
            var unitUnderTest = CreateRound(new Dictionary<Guid, Guid>
                {
                    {playerId, playerCard}
                },
                new Guid[] { });

            // Act
            unitUnderTest.Play(playerId,
                playerCard);

            // Assert
            Assert.IsTrue(unitUnderTest.Discarded.Contains(playerCard));
        }
    }
}