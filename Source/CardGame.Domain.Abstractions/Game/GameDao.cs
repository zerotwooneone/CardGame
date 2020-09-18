using System.Collections.Generic;

namespace CardGame.Domain.Abstractions.Game
{
    public class GameDao
    {
        public string Id { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Player3 { get; set; }
        public string Player4 { get; set; }
        public string RoundId { get; set; }
        public string EliminatedPlayer1 { get; set; }
        public string EliminatedPlayer2 { get; set; }
        public string EliminatedPlayer3 { get; set; }
        public string Deck { get; set; }
        public string TurnId { get; set; }
        public string CurrentPlayer { get; set; }
        public string Discard { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public int? Player3Score { get; set; }
        public int? Player4Score { get; set; }
        public string Player1Hand { get; set; }
        public string Player2Hand { get; set; }
        public string Player3Hand { get; set; }
        public string Player4Hand { get; set; }
        public bool Player1Protected { get; set; }
        public bool Player2Protected { get; set; }
        public bool Player3Protected { get; set; }
        public bool Player4Protected { get; set; }
    }


}