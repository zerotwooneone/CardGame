using System.Collections.Generic;

namespace CardGame.Application.DTO
{
    public class CommonKnowledgeGame
    {
        public string Id { get; set; }
        public IEnumerable<string> Players { get; set; }
        public Round Round { get; set; }
    }
}