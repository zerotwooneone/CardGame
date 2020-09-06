namespace CardGame.CommonModel.CommonState
{
    public interface ICommonStateModelFactory
    {
        ICommonStateModel Create(string gameId);
    }
}