using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Core.Hand;
using CardGame.Core.Turn;
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
        public void TradeHands_TargetNotProtected_TradesHands()
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
                new Guid[] { }
            );
            // Act
            var expected = card2;
            unitUnderTest.TradeHands(
                sourcePlayerId,
                targetPlayerId);
            var actual = unitUnderTest.GetPlayerHand(sourcePlayerId).Previous;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TradeHands_TargetProtected_DoesNotTradesHands()
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
            unitUnderTest.TradeHands(
                sourcePlayerId,
                targetPlayerId);
            var actual = unitUnderTest.GetPlayerHand(sourcePlayerId).Previous;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        //[TestMethod]
        //public void EliminatePlayer_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);
        //    Guid playerId = TODO;

        //    // Act
        //    unitUnderTest.EliminatePlayer(
        //        playerId);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void AddPlayerProtection_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);
        //    Guid playerId = TODO;

        //    // Act
        //    unitUnderTest.AddPlayerProtection(
        //        playerId);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void GetWinningPlayerId_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);
        //    GetCardValue getCardValue = TODO;

        //    // Act
        //    var result = unitUnderTest.GetWinningPlayerId(
        //        getCardValue);

        //    // Assert
        //    Assert.Fail();
        //}

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

        //[TestMethod]
        //public void Discard_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreateRound(TODO, TODO);
        //    Guid playCard = TODO;

        //    // Act
        //    unitUnderTest.Discard(
        //        playCard);

        //    // Assert
        //    Assert.Fail();
        //}
    }
}