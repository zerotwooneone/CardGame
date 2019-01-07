using System.Linq;
using CardGame.Core.Card;

namespace CardGame.Core.Turn
{
    public class Turn
    {
        public readonly Player.Player CurrentPlayer;
        public Card.Card Unplayable { get; private set; }

        public KnownPlayerHand KnownPlayerHand {get; private set; }

        public Turn(Player.Player currentPlayer)
        {
            CurrentPlayer = currentPlayer;
            KnownPlayerHand = null;
        }

        public void MarkUnplayable(Card.Card card)
        {
            Unplayable = card;
        }

        public void TradeHands(Player.Player target)
        {
            var targetHand = target.Hand;
            target.SetHand(CurrentPlayer.Hand);
            CurrentPlayer.SetHand(targetHand);
        }

        public void RevealHand(Player.Player target)
        {
            KnownPlayerHand = new KnownPlayerHand(target.Id, target.Hand.First().Id);
        }

        public void Init(Card.Card draw)
        {
            CurrentPlayer.SetHand(new Hand.Hand(CurrentPlayer.Hand.Previous, draw));
        }
    }
}