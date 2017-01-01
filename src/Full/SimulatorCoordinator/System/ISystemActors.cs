using System.Collections.Generic;
using Akka.Actor;

namespace Simulator.Coordinator
{
    public interface ISystemActors
    {
        IActorRef SimulationNotifier { get; }
        IActorRef TechnologyCoordinators { get; }
    }
}