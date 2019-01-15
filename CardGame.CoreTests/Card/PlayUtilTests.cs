using CardGame.Core.Card;
using CardGame.Core.Round;
using CardGame.Core.Turn;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace CardGame.CoreTests.Card
{
    [TestClass]
    public class PlayUtilTests
    {
        private MockRepository mockRepository;
        private Mock<IPlayRound> _round;
        private Mock<IPlayTurn> _turn;
        private Mock<IPlayCard> _card;


        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            _round = mockRepository.Create<IPlayRound>();
            _turn = mockRepository.Create<IPlayTurn>();
            _card = mockRepository.Create<IPlayCard>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.mockRepository.VerifyAll();
        }

        private PlayUtil CreatePlayUtil()
        {
            return new PlayUtil();
        }

        [TestMethod]
        public void Play_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var unitUnderTest = this.CreatePlayUtil();
            Guid playerId = Guid.Empty;
            
            CardValue drawnValue = CardValue.Baron;
            Guid? targetPlayer = null;
            CardValue? guessedCardvalue = null;
            CardValue? targetCard = null;

            _card
                .SetupGet(c => c.Id)
                .Returns(Guid.Empty);
            
            CardValue previousValue = CardValue.Princess;
            _card
                .SetupGet(c => c.Value)
                .Returns(previousValue);

            _round
                .Setup(r=>r.EliminatePlayer(It.IsAny<Guid>()))
                .Verifiable();
            _round
                .Setup(r=>r.Play(It.IsAny<Guid>(),It.IsAny<Guid>()))
                .Verifiable();

            // Act
            unitUnderTest.Play(
                playerId,
                _card.Object,
                _turn.Object,
                _round.Object,
                previousValue,
                drawnValue,
                targetPlayer,
                guessedCardvalue,
                targetCard);

            // Assert
            _round
                .Verify(r=>r.EliminatePlayer(playerId));
        }

        [TestMethod]
        public void PlayPrincess_StateUnderTest_PlayerEliminated()
        {
            // Arrange
            var unitUnderTest = this.CreatePlayUtil();
            Guid cardId = Guid.Parse("db023ad1-a72b-4929-a70d-c20efe0e9d1c");
            Guid playerId = Guid.Empty;

            _round
                .Setup(r=>r.EliminatePlayer(It.IsAny<Guid>()))
                .Verifiable();
            _round
                .Setup(r=>r.Play(It.IsAny<Guid>(),It.IsAny<Guid>()))
                .Verifiable();
            
            // Act
            unitUnderTest.PlayPrincess(
                cardId,
                playerId,
                _round.Object);

            // Assert
            _round
                .Verify(r=>r.EliminatePlayer(playerId));
        }

        [TestMethod]
        public void PlayPrincess_StateUnderTest_CardDiscarded()
        {
            // Arrange
            var unitUnderTest = this.CreatePlayUtil();
            Guid cardId = Guid.Parse("db023ad1-a72b-4929-a70d-c20efe0e9d1c");
            Guid playerId = Guid.Empty;

            _round
                .Setup(r=>r.EliminatePlayer(It.IsAny<Guid>()))
                .Verifiable();
            _round
                .Setup(r=>r.Play(It.IsAny<Guid>(),It.IsAny<Guid>()))
                .Verifiable();
            
            // Act
            unitUnderTest.PlayPrincess(
                cardId,
                playerId,
                _round.Object);

            // Assert
            _round
                .Verify(r=>r.Play(playerId,cardId));
        }

        [TestMethod]
        public void PlayKing_StateUnderTest_CallsTradeHands()
        {
            // Arrange
            var unitUnderTest = this.CreatePlayUtil();
            Guid cardId = Guid.Parse("db023ad1-a72b-4929-a70d-c20efe0e9d1c");
            Guid playerId = Guid.Empty;
            Guid targetId = Guid.Parse("d8ee9f0c-3e4d-41b9-9871-8172f78e0c2b");
            
            _round
                .Setup(r=>r.TradeHands(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Guid.Parse("89e99225-35c0-4a63-a62e-f61aa65a1451"));
            _round
                .Setup(r=>r.Play(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Verifiable();

            // Act
            unitUnderTest.PlayKing(
                cardId,
                playerId,
                targetId,
                _round.Object);

            // Assert
            _round
                .Verify(r=>r.TradeHands(playerId, targetId));
        }

        //[TestMethod]
        //public void PlayPrince_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreatePlayUtil();
        //    Guid cardId = TODO;
        //    Guid playerId = TODO;
        //    Guid targetId = TODO;
        //    IPlayRound round = TODO;

        //    // Act
        //    unitUnderTest.PlayPrince(
        //        cardId,
        //        playerId,
        //        targetId,
        //        round);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void PlayHandmaid_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreatePlayUtil();
        //    Guid cardId = TODO;
        //    Guid playerId = TODO;
        //    IPlayRound round = TODO;

        //    // Act
        //    unitUnderTest.PlayHandmaid(
        //        cardId,
        //        playerId,
        //        round);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void PlayBaron_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreatePlayUtil();
        //    Guid cardId = TODO;
        //    Guid playerId = TODO;
        //    Guid targetId = TODO;
        //    CardValue targetHand = TODO;
        //    IPlayRound round = TODO;

        //    // Act
        //    unitUnderTest.PlayBaron(
        //        cardId,
        //        playerId,
        //        targetId,
        //        targetHand,
        //        round);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void PlayPriest_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreatePlayUtil();
        //    Guid cardId = TODO;
        //    Guid playerId = TODO;
        //    Guid targetId = TODO;
        //    CardValue targetHand = TODO;
        //    IPlayTurn turn = TODO;
        //    IPlayRound round = TODO;

        //    // Act
        //    var result = unitUnderTest.PlayPriest(
        //        cardId,
        //        playerId,
        //        targetId,
        //        targetHand,
        //        turn,
        //        round);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void PlayGuard_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreatePlayUtil();
        //    Guid cardId = TODO;
        //    Guid playerId = TODO;
        //    Guid targetId = TODO;
        //    CardValue targetHand = TODO;
        //    IPlayRound round = TODO;
        //    CardValue guess = TODO;

        //    // Act
        //    unitUnderTest.PlayGuard(
        //        cardId,
        //        playerId,
        //        targetId,
        //        targetHand,
        //        round,
        //        guess);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void RequiresTargetPlayerToPlay_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreatePlayUtil();
        //    CardValue cardValue = TODO;

        //    // Act
        //    var result = unitUnderTest.RequiresTargetPlayerToPlay(
        //        cardValue);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void RequiresTargetHandToPlay_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreatePlayUtil();
        //    CardValue cardValue = TODO;

        //    // Act
        //    var result = unitUnderTest.RequiresTargetHandToPlay(
        //        cardValue);

        //    // Assert
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void RequiresGuessedCardToPlay_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var unitUnderTest = this.CreatePlayUtil();
        //    CardValue cardValue = TODO;

        //    // Act
        //    var result = unitUnderTest.RequiresGuessedCardToPlay(
        //        cardValue);

        //    // Assert
        //    Assert.Fail();
        //}
    }
}
