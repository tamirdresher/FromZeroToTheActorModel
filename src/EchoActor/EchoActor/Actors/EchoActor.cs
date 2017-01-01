using Akka.Actor;
using EchoActor.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchoActor.Actors
{
    class EchoActor:ReceiveActor
    {
        public EchoActor()
        {
            Receive<EchoMessage>(m => 
            {
                Sender.Tell(m);
            });
        }
    }
}
