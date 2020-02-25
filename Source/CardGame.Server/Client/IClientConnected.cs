namespace CardGame.Server.Client
{
    public interface IClientConnected
    {
        string X { get; }
    }

    public class ClientConnected : IClientConnected
    {
        public string X { get; set; }
    }
}