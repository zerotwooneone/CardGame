namespace CardGame.Application.DTO
{
    public class PlayerDto
    {
        public string Hand { get; set; }
        public int? Score { get; set; }
        public bool Protected { get; set; }
    }
}