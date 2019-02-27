using Akka.Actor;
using EchoActor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Event;

namespace EchoActor.Actors
{
    class EchoActor : ReceiveActor
    {
        public EchoActor()
        {
            Receive<EchoMessage>(m => 
            {
                Context.GetLogger().Info($"Got message '{m.Message}'");
                Sender.Tell(m);
            });
        }
    }
}
