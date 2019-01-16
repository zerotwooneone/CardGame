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

            var cardId = Guid.Parse("db59067e-2e95-48a7-a134-a793d8b50a0a");
            _card
                .SetupGet(c => c.Id)
                .Returns(cardId);
            
            CardValue previousValue = CardValue.Princess;
            _card
                .SetupGet(c => c.Value)
                .Returns(previousValue);

            _round
                .Setup(r=>r.PlayPrincess(It.IsAny<Guid>(), It.IsAny<Guid>()))
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
                .Verify(r=>r.PlayPrincess(cardId, playerId));
        }
        
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
