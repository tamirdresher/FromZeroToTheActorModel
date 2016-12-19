using System.Collections.Generic;
using Akka.Actor;

namespace Simulator.Coordinator
{
    public interface ISystemActors
    {
        IActorRef SimulationNotifier { get; }
        IEnumerable<ActorSelection> TechnologyCoordinators { get; }
    }
}