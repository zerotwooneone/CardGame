using System;
using System.Threading.Tasks;
using CardGame.Peer;
using CardGame.Peer.MessagePipe;
using Newtonsoft.Json;
using Unity;
using Unity.Lifetime;

namespace CardGamePeer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var container = new UnityContainer();

            container.RegisterType<Startup>(new ContainerControlledLifetimeManager());
            
            var startup = container.Resolve<Startup>();

            startup.Setup(container);

            startup.Configure();

            var programViewmodel = container.Resolve<ProgramViewmodel>(); //yuck, fix this later
            programViewmodel.OutputObservable.Subscribe(s => Console.WriteLine(s));
            startup.Start(programViewmodel);

            var outputService = container.Resolve<OutputService>();

            try
            {
                using (var c = container.Resolve<MessageClient>())
                {
                    c.MessageObservable.Subscribe(m =>
                    {
                        outputService.WriteLine($"Message Received:{JsonConvert.SerializeObject(m)}");
                    });
                    c.Connect(TimeSpan.FromSeconds(1.0));
                    var message = new Message { Id = Guid.NewGuid() };
                    outputService.WriteLine($"Sending: {JsonConvert.SerializeObject(message)}");
                    c.GetResponse(message);

                    Task.Delay(TimeSpan.FromSeconds(2)).Wait();
                }
            }
            catch (TimeoutException e) when (e.Message.StartsWith("The operation has timed out."))
            {
                outputService.WriteLine("Client failed to connect. Assuming server role");
                //using (
                var s = container.Resolve<MessageServer>();
                //){
                s.ClientConnectedObservable
                    //.Take(1)
                    //.Delay(TimeSpan.FromMilliseconds(1))
                    .Subscribe(i =>
                {
                    outputService.WriteLine("Client Connected");
                    var message = new Message { Id = Guid.NewGuid() };
                    outputService.WriteLine($"Sending: {JsonConvert.SerializeObject(message)}");
                    s.SendMessage(message);
                });
                s.MessageObservable.Subscribe(m =>
                {
                    outputService.WriteLine($"Message Received:{JsonConvert.SerializeObject(m)}");
                });
                Task.Delay(TimeSpan.FromSeconds(2)).Wait();
                //}
            }

            do
            {
                //Thread.Sleep(TimeSpan.FromMilliseconds(1));
                Task.Delay(TimeSpan.FromMilliseconds(1)).Wait();
            } while (true);
        }


    }
}
