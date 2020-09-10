namespace CardGame.CommonModel.Bus
{
    public class ServiceCall
    {
        public string Service { get; set; }
        public string Method { get; set; }
        public object Param { get; set; }
    }
}