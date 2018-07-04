namespace CardGame.Peer.NamedPipes
{
    public class NamedPipeConfig
    {
        /// <summary>
        /// The name of the pipe to use
        /// </summary>
        public string PipeName { get; set; }
        /// <summary>
        /// The name of the server to which we should connect. Use "." for local.
        /// </summary>
        public string ServerName { get; set; }
    }
}