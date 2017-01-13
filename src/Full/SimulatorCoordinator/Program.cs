using System;
using Akka.Actor;
using Simulator.Coordinator.Actors;

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
               
                actorSystem.WhenTerminated.Wait();
                Console.ReadLine();
            }
        }

    }

   
}
