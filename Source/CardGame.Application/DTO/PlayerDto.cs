using System.Collections.Generic;

namespace CardGame.Application.DTO
{
    public class PlayerDto
    {
        public IEnumerable<CardDto> Hand { get; set; }
        public int? Score { get; set; }
        public bool Protected { get; set; }
    }
}