using System;
using System.Collections.Generic;

namespace CardGame.CommonModel.Bus
{
    public class CommonGameStateChanged : IEvent
    {
        public Guid? CurrentPlayer { get; set; }
        public bool? Player1InRound { get; set; }
        public bool? Player2InRound { get; set; }
        public bool? Player3InRound { get; set; }
        public bool? Player4InRound { get; set; }
        public int? Player1Score { get; set; }
        public int? Player2Score { get; set; }
        public int? Player3Score { get; set; }
        public int? Player4Score { get; set; }
        public int? Round { get; set; }
        public int? Turn { get; set; }
        public IEnumerable<string> Discard { get; set; }
        public Guid? WinningPlayer { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid GameId { get; set; }
        public int? DrawCount { get; set; }
    }
}