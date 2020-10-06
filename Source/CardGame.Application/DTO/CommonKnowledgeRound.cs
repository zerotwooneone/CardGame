using System.Collections.Generic;

namespace CardGame.Application.DTO
{
    public class CommonKnowledgeRound
    {
        public int Id { get; set; }
        public CommonKnowledgeTurn Turn { get; set; }
        public IEnumerable<string> Discard { get; set; }
        public int DeckCount { get; set; }
        public IEnumerable<string> PlayerOrder { get; set; }
    }
}