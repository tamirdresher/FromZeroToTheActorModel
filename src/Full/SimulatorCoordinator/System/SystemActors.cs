using System.Collections.Generic;
using System.Configuration;
using Akka.Actor;

namespace Simulator.Coordinator
{
    class SystemActors : ISystemActors
    {
        private readonly ActorSystem _system;

        public SystemActors(ActorSystem system)
        {
            _system = system;
        }

        public IEnumerable<ActorSelection> TechnologyCoordinators =>
            new[]
            {
                _system.ActorSelection(ConfigurationManager.AppSettings["TechASystem"] + "/user/coordinator")
            };

        public IActorRef SimulationNotifier { get; set; }
    }
}