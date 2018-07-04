using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CardGamePeer;
using Newtonsoft.Json;
using ProtoBuf;

namespace CardGame.Peer.Server
{
    public class HelloWorldServer
    {
        public const string MytestPipeName = "MyTest.Pipe";
        private readonly OutputService _outputService;
        private readonly ServerPipe _serverPipe;
        public IObservable<int> ClientConnectedObservable { get; }

        public HelloWorldServer(OutputService outputService)
        {
            _outputService = outputService;
            _serverPipe = new ServerPipe(MytestPipeName);
            _serverPipe.DataReadObservable.Subscribe(_serverPipe_ReadDataEvent);
            ClientConnectedObservable = Observable.FromEvent<EventHandler<EventArgs>, int>(
                onNextHandler =>
                {
                    void HandleEvent(object x, EventArgs y)
                    {
                        onNextHandler(0);
                    }
                    return HandleEvent;
                },
                h => _serverPipe.GotConnectionEvent += h,
                h => _serverPipe.GotConnectionEvent -= h);

        }

        private void _serverPipe_ReadDataEvent(PipeEventArgs e)
        {
            var memoryStream = new MemoryStream(e.m_pData, 0, e.m_nDataLen);
            var message = Serializer.Deserialize<Message>(memoryStream);
            _outputService.WriteLine($"Message: {JsonConvert.SerializeObject(message)}");
        }

        public Task SendMessage(Message message)
        {
            return _serverPipe.SendCommandAsync(message);
        }
    }

    public class HelloWorldClient
    {
        private const string PipeServername = ".";
        private readonly OutputService _outputService;
        private readonly ClientPipe _clientPipe;

        public HelloWorldClient(OutputService outputService)
        {
            _outputService = outputService;
            _clientPipe = new ClientPipe(PipeServername, HelloWorldServer.MytestPipeName);
            _clientPipe.DataReadObservable.Subscribe(_clientPipe_ReadDataEvent);
        }

        private void _clientPipe_ReadDataEvent(PipeEventArgs e)
        {
            var memoryStream = new MemoryStream(e.m_pData, 0, e.m_nDataLen);
            var message = Serializer.Deserialize<Message>(memoryStream);
            _outputService.WriteLine($"Message: {JsonConvert.SerializeObject(message)}");
        }

        public Task SendMessage(Message message)
        {
            return _clientPipe.SendCommandAsync(message);
        }

        public void Connect(TimeSpan? timeout = null)
        {
            _clientPipe.Connect(timeout);
        }
    }

    public interface MessageSender
    {
        Task SendCommandAsync(Message pCmd);
    }

    public class ClientPipe : BasicPipe
    {
        NamedPipeClientStream m_pPipe;

        public ClientPipe(string szServerName, string szPipeName)
        {
            m_szPipeName = szPipeName; // debugging
            m_pPipe = new NamedPipeClientStream(szServerName, szPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            base.SetPipeStream(m_pPipe); // inform base class what to read/write from
        }

        public void Connect(TimeSpan? timeout = null)
        {
            Console.WriteLine($"Pipe {m_szPipeName} connecting to server");
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

    public class ServerPipe : BasicPipe
    {
        public event EventHandler<EventArgs> GotConnectionEvent;

        NamedPipeServerStream m_pPipe;

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
            m_pPipe.EndWaitForConnection(pAsyncResult);

            Console.WriteLine("Server Pipe " + m_szPipeName + " got a connection");

            GotConnectionEvent?.Invoke(this, new EventArgs());

            // lodge the first read request to get us going
            //
            StartReadingAsync();
        }
    }


    public abstract class BasicPipe : MessageSender, IDisposable
    {
        public static int MaxLen = 1024 * 1024; // why not
        protected string m_szPipeName;

        public IObservable<PipeEventArgs> DataReadObservable => _dataReadSubject;
        public IObservable<int?> PipeClosedObservable => _pipeClosedSubject;

        //protected byte[] m_pPipeBuffer = new byte[BasicPipe.MaxLen];

        PipeStream m_pPipeStream;
        private readonly ISubject<PipeEventArgs> _dataReadSubject;
        private readonly ISubject<int?> _pipeClosedSubject;

        protected BasicPipe()
        {
            _dataReadSubject = new Subject<PipeEventArgs>();
            _pipeClosedSubject = new Subject<int?>();
        }

        protected void SetPipeStream(PipeStream p)
        {
            m_pPipeStream = p;
        }

        public void Close()
        {
            m_pPipeStream.WaitForPipeDrain();
            m_pPipeStream.Close();
            m_pPipeStream.Dispose();
            m_pPipeStream = null;
        }

        // called when Server pipe gets a connection, or when Client pipe is created
        public void StartReadingAsync()
        {
            Console.WriteLine($"Pipe {m_szPipeName} calling ReadAsync");

            // okay we're connected, now immediately listen for incoming buffers
            //
            byte[] pBuffer = new byte[MaxLen];
            m_pPipeStream.ReadAsync(pBuffer, 0, MaxLen).ContinueWith(t =>
            {
                Console.WriteLine($"Pipe {m_szPipeName} finished a read request");

                int ReadLen = t.Result;
                if (ReadLen == 0)
                {
                    Console.WriteLine("Got a null read length, remote pipe was closed");
                    _pipeClosedSubject.OnNext(null);
                    return;
                }

                _dataReadSubject.OnNext(new PipeEventArgs(pBuffer, ReadLen));

                // lodge ANOTHER read request
                //
                StartReadingAsync();

            });
        }

        protected Task WriteByteArray(byte[] pBytes)
        {
            // this will start writing, but does it copy the memory before returning?
            return m_pPipeStream.WriteAsync(pBytes, 0, pBytes.Length);
        }

        public Task SendCommandAsync(Message pCmd)
        {
            Console.WriteLine($"Pipe {m_szPipeName}, writing Message {JsonConvert.SerializeObject(pCmd)}");
            var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, pCmd);
            memoryStream.Position = 0;
            Task t = WriteByteArray(memoryStream.ToArray());
            return t;
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

    public class PipeEventArgs
    {
        public byte[] m_pData;
        public int m_nDataLen;

        public PipeEventArgs(byte[] pData, int nDataLen)
        {
            // is this a copy, or an alias copy? I can't remember right now.
            m_pData = pData;
            m_nDataLen = nDataLen;
        }
    }

    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public int Id { get; set; }
    }

    [ProtoContract]
    public class Response
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}