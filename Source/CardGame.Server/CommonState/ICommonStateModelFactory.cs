namespace CardGame.Server.CommonState
{
    public interface ICommonStateModelFactory
    {
        ICommonStateModel Create(string gameId);
    }
}