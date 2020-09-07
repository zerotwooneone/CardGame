using System.Collections.Generic;

namespace CardGame.Application.DTO
{
    public class Round
    {
        public string Id { get; set; }
        public IEnumerable<string> EliminatedPlayers { get; set; }
        public Turn Turn { get; set; }
        public int DeckCount { get; set; }
        public IEnumerable<string> Discard { get; set; }
    }
}