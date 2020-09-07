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
        public int DeckCount { get; set; }
        public string TurnId { get; set; }
        public string CurrentPlayer { get; set; }
        public IEnumerable<string> Discard { get; set; }
    }
}