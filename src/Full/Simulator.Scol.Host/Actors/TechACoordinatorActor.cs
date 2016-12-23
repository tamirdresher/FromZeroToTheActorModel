using System;
using Akka.Actor;
using Akka.Event;
using Simulator.Coordinator.Actors;
using Simulator.Messages.Messages;
using Simulator.Messages.Messages.Errors;

namespace Simulator.TechnologyA.Actors
{
    public class TechACoordinatorActor : ReceiveActor
    {
        private Simulation _currentSimulation;

        public TechACoordinatorActor()
        {
            Become(FreeForSimulations);
        }

        private void FreeForSimulations()
        {
            Receive<StartSimulation>(IsAcceptableTechnology,
                m =>
                {
                    Context.GetLogger().Info($"Start Simulation {m.ProjectId}");
                    _currentSimulation = CreateSimulation(m);
                    _currentSimulation.Actor.Tell(m);
                    Become(Simulating);
                });
        }
        private void Simulating()
        {
            Receive<StopSimulation>(stop => stop.ProjectId == _currentSimulation.ProjectId,
                m => StopRunningSimulation(m));
            Receive<PauseSimulation>(pauseSimulation => pauseSimulation.ProjectId == _currentSimulation.ProjectId,
                m => PauseRunningSimulation(m));
            Receive<SimulationFinished>(_ => _currentSimulation.Actor.Equals(Sender),
                m =>
                {
                    _currentSimulation.Requester.Tell(m);
                    ClearSimulation();
                    Become(FreeForSimulations);
                });
            ReceiveAny(m =>
            {
                Context.GetLogger().Warning($"Got unexpected message {m.ToString()}");
            });
            Sender.Tell(new SimulationStarted { ProjectId = _currentSimulation.ProjectId });
        }
        private void Paused()
        {
            FreeForSimulations();
            Receive<ResumeSimulation>(IsAcceptableTechnology,
                m =>
                {
                    Context.GetLogger().Info($"Resume Simulation {m.ProjectId}");

                    if (IsCurrentSimulation(m))
                    {
                        _currentSimulation.Actor.Tell(new ResumeSimulation { ProjectId = m.ProjectId });
                    }
                    else
                    {
                        StopRunningSimulation(new StopSimulation { ProjectId = _currentSimulation.ProjectId });
                        _currentSimulation = CreateSimulation(m);
                        _currentSimulation.Actor.Tell(m);
                    }
                    Become(Simulating);
                });
            Receive<StopSimulation>(IsCurrentSimulation, m => StopRunningSimulation(m));
        }
        
        private void PauseRunningSimulation(PauseSimulation m)
        {
            Context.GetLogger().Info($"Pause Simulation {m.ProjectId}");
            _currentSimulation.Actor.Tell(m);
            Sender.Tell(new SimulationPaused() { ProjectId = m.ProjectId });
            Become(Paused);
        }
        private void StopRunningSimulation(StopSimulation m)
        {
            Context.GetLogger().Info($"Stop Simulation {m.ProjectId}");

            _currentSimulation.Actor.Tell(m);
            ClearSimulation();
            Sender.Tell(new SimulationStopped { ProjectId = m.ProjectId });
            Become(FreeForSimulations);
        }
        private void ClearSimulation()
        {
            _currentSimulation = Simulation.Empty;
        }

        protected override void PreStart()
        {
            base.PreStart();
            Console.WriteLine(Context.Self);
        }

        private Simulation CreateSimulation(IProjectSimulationMessage m)
        {
            return new Simulation(m.ProjectId,
                Context.ActorOf(Props.Create(() => new TechASimulationActor(m.ProjectId)), m.ProjectId.ToString()),
                Sender);
        }
        private bool IsCurrentSimulation(IProjectSimulationMessage m)
        {
            return m.ProjectId != _currentSimulation?.ProjectId;
        }
        private bool IsAcceptableTechnology(ITechnologyAwareMessage msg)
        {
            Context.GetLogger().Info($"IsAcceptableTechnology - msg:{msg.GetType()} Technology:{msg.Technology} ");
            return msg.Technology == Technology.TechnologyA;
        }
    }
}

