using System;
using Akka.Actor;
using Simulator.Messages.Messages;

namespace Simulator.TechnologyA.Actors
{
    class Simulation
    {
        public Simulation(Guid project, IActorRef simulationActor, IActorRef requester)
        {
            ProjectId = project;
            Actor = simulationActor;
            Requester = requester;
        }
        public IActorRef Actor { get;  }
        public IActorRef Requester { get; }
        public Guid ProjectId { get;  }

        public static Simulation Empty { get; } = new Simulation(project: Guid.Empty,
            simulationActor: ActorRefs.Nobody,
            requester: ActorRefs.NoSender);
    }
}