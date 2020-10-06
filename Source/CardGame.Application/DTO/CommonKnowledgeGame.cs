using System;
using System.Collections.Generic;

namespace CardGame.Application.DTO
{
    public class CommonKnowledgeGame
    {
        public Guid Id { get; set; }
        public IEnumerable<CommonKnowledgePlayer> Players { get; set; }
        public CommonKnowledgeRound Round { get; set; }
        public Guid? WinningPlayer { get; set; }
    }
}