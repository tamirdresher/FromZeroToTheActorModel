using System;
using Akka.Actor;
using Simulator.Coordinator.Actors;
using Simulator.Messages.Messages;
using Simulator.Messages.Messages.SimulationQueue;

namespace Simulator.Coordinator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var actorSystem = ActorSystem.Create("simulator"))
            {
                var firstProj = Guid.NewGuid();
                var secondProj   = Guid.NewGuid();
                var notifierProps = Props.Create(() => new SimulationNotifierActor());

                var systemActors = new SystemActors(actorSystem)
                {
                    SimulationNotifier = actorSystem.ActorOf(notifierProps)
                };
                
                var props = Props.Create(() => new CoordinatorActor(systemActors));
                var coordinatorActor = actorSystem.ActorOf(props,"SimulationCoordinator");

                var coActorSelection = actorSystem.ActorSelection("akka.tcp://TechnologyA@localhost:8091/user/coordinator");
                coActorSelection.Tell(new StartSimulation());

                actorSystem.WhenTerminated.Wait();
                Console.ReadLine();
            }
        }

    }

   
}
