using System.Collections.Generic;

namespace CardGame.Application.DTOs
{
    public class DeckDefinitionDto
    {
        public required IEnumerable<CardDto> Cards { get; set; }
        public required string BackAppearanceId { get; set; }
    }
}
