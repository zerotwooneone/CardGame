using System;
using System.IO;
using System.IO.Pipes;

namespace CardGame.Peer.Server
{
    public class ServerPipe : PipeBase
    {
        public event EventHandler<EventArgs> GotConnectionEvent;

        readonly NamedPipeServerStream m_pPipe;

        public ServerPipe(string szPipeName)
        {
            m_szPipeName = szPipeName;
            m_pPipe = new NamedPipeServerStream(
                szPipeName,
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous);
            base.SetPipeStream(m_pPipe);
            m_pPipe.BeginWaitForConnection(StaticGotPipeConnection, this);
        }

        static void StaticGotPipeConnection(IAsyncResult pAsyncResult)
        {
            ServerPipe pThis = pAsyncResult.AsyncState as ServerPipe;
            pThis.GotPipeConnection(pAsyncResult);
        }

        void GotPipeConnection(IAsyncResult pAsyncResult)
        {
            try
            {
                m_pPipe.EndWaitForConnection(pAsyncResult);
            }
            catch (IOException ioException) when(ioException.Message.StartsWith("The pipe has been ended"))
            {
                Close();
                return;
            }

            GotConnectionEvent?.Invoke(this, new EventArgs());

            // lodge the first read request to get us going
            //
            StartReadingAsync();
        }
    }
}