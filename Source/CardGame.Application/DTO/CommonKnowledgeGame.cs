using System;
using System.Collections.Generic;

namespace CardGame.Application.DTO
{
    public class CommonKnowledgeGame
    {
        public string Id { get; set; }
        public IEnumerable<string> Players { get; set; }
        public Round Round { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
        public int? Player3Score { get; set; }
        public int? Player4Score { get; set; }
        public Guid? WinningPlayer { get; set; }
    }
}