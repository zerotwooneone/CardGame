using System;
using System.IO;
using System.IO.Pipes;
using System.Reactive.Subjects;

namespace CardGame.Peer.NamedPipes
{
    public class ServerPipe : PipeBase
    {
        public IObservable<bool> ConnectedObservable { get; set; }
        private readonly ISubject<bool> _connectedSubject;

        readonly NamedPipeServerStream m_pPipe;

        public ServerPipe(string szPipeName)
        {
            _connectedSubject = new BehaviorSubject<bool>(false);
            ConnectedObservable = _connectedSubject;
            PipeClosedObservable.Subscribe(ni => _connectedSubject.OnNext(false));
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

            _connectedSubject.OnNext(true);

            // lodge the first read request to get us going
            //
            StartReadingAsync();
        }
    }
}