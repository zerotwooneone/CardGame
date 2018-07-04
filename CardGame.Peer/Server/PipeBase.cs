using System;
using System.IO;
using System.IO.Pipes;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ProtoBuf;

namespace CardGame.Peer.Server
{
    public abstract class PipeBase : IDisposable
    {
        public static int MaxLen = 1024 * 1024; // why not
        protected string m_szPipeName;

        public IObservable<byte[]> DataReadObservable => _dataReadSubject;
        public IObservable<int?> PipeClosedObservable => _pipeClosedSubject;
        
        PipeStream m_pPipeStream;
        private readonly ISubject<byte[]> _dataReadSubject;
        private readonly ISubject<int?> _pipeClosedSubject;

        protected PipeBase()
        {
            _dataReadSubject = new Subject<byte[]>();
            _pipeClosedSubject = new Subject<int?>();
        }

        protected void SetPipeStream(PipeStream p)
        {
            m_pPipeStream = p;
        }

        public void Close()
        {
            try
            {
                m_pPipeStream.WaitForPipeDrain();
            }
            catch (ObjectDisposedException objectDisposedException) when(objectDisposedException.Message.StartsWith("Cannot access a closed pipe."))
            {
                return;
            }
            
            m_pPipeStream.Close();
            //m_pPipeStream.Dispose();
            //m_pPipeStream = null;
            _pipeClosedSubject.OnNext(null);
        }

        // called when Server pipe gets a connection, or when Client pipe is created
        public void StartReadingAsync()
        {
            // okay we're connected, now immediately listen for incoming buffers
            //
            byte[] pBuffer = new byte[MaxLen];
            m_pPipeStream.ReadAsync(pBuffer, 0, MaxLen).ContinueWith(t =>
            {
                int readLen = t.Result;
                if (readLen == 0)
                {
                    Close();
                    return;
                }
                Array.Resize(ref pBuffer, readLen);
                _dataReadSubject.OnNext(pBuffer);

                // lodge ANOTHER read request
                //
                StartReadingAsync();

            });
        }

        public Task WriteByteArray(byte[] pBytes)
        {
            // this will start writing, but does it copy the memory before returning?
            return m_pPipeStream.WriteAsync(pBytes, 0, pBytes.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_pPipeStream?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}