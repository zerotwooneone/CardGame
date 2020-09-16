using System;

namespace CardGame.CommonModel.Bus
{
    public class PlayRequest: IRequest
    {
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; } 
        public int CardStrength { get; set; }
        public int CardVarient { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid TargetId { get; set; }
        public int GuessValue { get; set; }
    }
}