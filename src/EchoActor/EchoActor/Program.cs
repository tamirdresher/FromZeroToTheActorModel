using Akka.Actor;
using EchoActor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoActor
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var sys = ActorSystem.Create("echo"))
            {
                IActorRef echoActor=sys.ActorOf<Actors.EchoActor>("echoActor");
                var msg=echoActor.Ask<EchoMessage>(new EchoMessage { Message = "Hello World" }).Result;
                Console.WriteLine(msg.Message);
                sys.WhenTerminated.Wait();
            }

        }
    }
}
