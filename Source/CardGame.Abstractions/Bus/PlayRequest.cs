using System;

namespace CardGame.CommonModel.Bus
{
    public class PlayRequest
    {
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; } 
        public int CardStrength { get; set; }
        public int CardVarient { get; set; }
        public Guid EventId { get; set; }
        public Guid? CorrelationId { get; set; }
    }
}