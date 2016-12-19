using System;
using Akka.Actor;
using Akka.Event;
using Simulator.Messages.Messages;
using Simulator.Messages.Messages.Errors;

namespace Simulator.TechnologyA.Actors
{
    public class TechACoordinator : ReceiveActor 
    {
        private Guid _currentSimulation;

        public TechACoordinator()
        {
            Become(FreeForSimulations);
        }

        private void FreeForSimulations()
        {
            Receive<StartSimulation>(m =>
            {
                Context.GetLogger().Info($"Start Simulation {m.ProjectId}");
                if (m.Technology!=Technology.TechnologyA)
                {
                    Sender.Tell(new InvalidSimualtionRequest("Only SCOL Technology is supported by this actor"));
                    return;
                }
                Become(()=>Simulating(m.ProjectId));
            });
            Receive<ResumeSimulation>(m =>
            {
                Context.GetLogger().Info($"Resume Simulation {m.ProjectId}");

                if (m.Technology != Technology.TechnologyA)
                {
                    Sender.Tell(new InvalidSimualtionRequest("Only SCOL Technology is supported by this actor"));
                    return;
                }
                Become(() => Simulating(m.ProjectId));
            });
        }

        private void Simulating(Guid project)
        {
            _currentSimulation = project;

            Receive<StopSimulation>(stop => stop.ProjectId == _currentSimulation,
                m =>
                {
                    Context.GetLogger().Info($"Stop Simulation {m.ProjectId}");

                    _currentSimulation = Guid.Empty;
                    Sender.Tell(new SimulationStopped(){ProjectId = _currentSimulation});
                    Become(FreeForSimulations);
                });
            Receive<PauseSimulation>(pauseSimulation => pauseSimulation.ProjectId == _currentSimulation,
               m =>
                {
                    Context.GetLogger().Info($"Pause Simulation {m.ProjectId}");

                    _currentSimulation = Guid.Empty;
                    Sender.Tell(new SimulationPaused(){ProjectId = _currentSimulation});
                    Become(FreeForSimulations);
                });

            ReceiveAny(m =>
            {
                Context.GetLogger().Warning($"Got unexpected message {m.ToString()}");
            });
            Sender.Tell(new SimulationStarted() { ProjectId = _currentSimulation });

        }

        protected override void PreStart()
        {
            base.PreStart();
            Console.WriteLine(Context.Self);
        }
    }
}

