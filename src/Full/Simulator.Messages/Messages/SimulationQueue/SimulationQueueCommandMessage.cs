using System;
using Akka.Actor;

namespace Simulator.Messages.Messages.SimulationQueue
{
    public abstract class SimulationQueueCommandMessage
    {
        public Guid ProjectId { get; set; }
        public IActorRef OriginalSender { get; set; }
    }
}