using System;
using Akka.Actor;

namespace Simulator.Messages.Messages.SimulationQueue
{
    public class AddNewSimulation
    {
        public Guid ProjId { get; set; }
        public Technology Technology { get; set; }
        public IActorRef OriginalSender { get; set; }
    }

    public class QueueEntry   
    {
        public Guid ProjId { get; set; }
        public Technology Technology { get; set; }
        public SimulationState SimulationState { get; set; }
    }
}