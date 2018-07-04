using System;
using System.IO.Pipes;

namespace CardGame.Peer.Server
{
    public class ClientPipe : PipeBase
    {
        readonly NamedPipeClientStream m_pPipe;

        public ClientPipe(string szServerName, string szPipeName)
        {
            m_szPipeName = szPipeName; // debugging
            m_pPipe = new NamedPipeClientStream(szServerName, szPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            base.SetPipeStream(m_pPipe); // inform base class what to read/write from
        }

        public void Connect(TimeSpan? timeout = null)
        {
            if (timeout == null)
            {
                m_pPipe.Connect(); // doesn't seem to be an async method for this routine. just a timeout.
            }
            else
            {
                m_pPipe.Connect((int)timeout.Value.TotalMilliseconds);
            }

            StartReadingAsync();
        }
    }
}