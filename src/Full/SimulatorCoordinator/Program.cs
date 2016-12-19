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
                var notifierProps = Props.Create(() => new SImulationNotifierActor());

                var systemActors = new SystemActors(actorSystem)
                {
                    SimulationNotifier = actorSystem.ActorOf(notifierProps)
                };
                
                var props = Props.Create(() => new CoordinatorActor(systemActors));
                var coordinatorActor = actorSystem.ActorOf(props,"SimulationCoordinator");
                //coordinatorActor.Tell(new StartSimulation {Technology = Technology.SCOL, ProjectId = firstProj}, systemActors.SimulationNotifier);
                //Thread.Sleep(5000);
                //coordinatorActor.Tell(new PauseSimulation{ ProjectId = firstProj }, systemActors.SimulationNotifier);
                //Thread.Sleep(5000);
                //coordinatorActor.Tell(new ResumeSimulation{ ProjectId = firstProj }, systemActors.SimulationNotifier);
                //Thread.Sleep(5000);
                //coordinatorActor.Tell(new PauseSimulation { ProjectId = firstProj }, systemActors.SimulationNotifier);
                //Thread.Sleep(5000);
                //coordinatorActor.Tell(new StartSimulation { Technology = Technology.SCOL, ProjectId = secondProj }, systemActors.SimulationNotifier);
                //Thread.Sleep(5000);
                //coordinatorActor.Tell(new StopSimulation() { ProjectId = firstProj }, systemActors.SimulationNotifier);


                //Thread.Sleep(5000);
                //Console.WriteLine($"proj id {firstProj}");
                //systemActors.TechnologyCoordinators.ForEach(c=>c.Tell(new StartSimulation { Technology = Technology.SCOL , ProjectId = firstProj}));


                actorSystem.WhenTerminated.Wait();
                Console.ReadLine();
            }
        }

    }

    internal class RunningSimulationActor
    {
        public IActorRef ActorRef { get; set; }
        public Guid ProjectId { get; set; }
    }
}
