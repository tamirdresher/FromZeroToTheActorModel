using System.Collections.Generic;
using System.Configuration;
using Akka.Actor;
using Akka.Routing;

namespace Simulator.Coordinator
{
    class SystemActors : ISystemActors
    {
        private readonly ActorSystem _system;

        public SystemActors(ActorSystem system)
        {
            _system = system;
            TechnologyCoordinators= _system.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "Technologies");

        }

        public IActorRef TechnologyCoordinators { get; }

        public IActorRef SimulationNotifier { get; set; }
    }
}