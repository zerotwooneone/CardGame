using System;

namespace CardGame.Application.DTO
{
    public class PlayRequest
    {
        public Guid PlayerId { get; set; } 
        public int CardStrength { get; set; }
        public int CardVarient { get; set; }
        public Guid CorrelationId { get; set; }
        public Guid TargetId { get; set; }
        public int GuessValue { get; set; }
    }
}