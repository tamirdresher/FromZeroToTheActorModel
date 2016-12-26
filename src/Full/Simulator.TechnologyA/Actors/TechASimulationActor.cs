using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Simulator.Messages;
using Simulator.Messages.Messages;

namespace Simulator.Coordinator.Actors
{
    class TechASimulationActor : ReceiveActor
    {
        private IActorRef _parent;
        private Guid _projectId;
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public TechASimulationActor(Guid project)
        {
            _projectId = project;
            Become(ReadyToSimulate);
        }

        private void ReadyToSimulate()
        {
            Receive<StartSimulation>(m =>
            {
                _parent = Context.Sender;
                _projectId = m.ProjectId;
                Console.WriteLine($"Actor in path:{Context.Self.Path} - starting simulation");
                Become(WaitingForSimulationToFinish);
            });
            Receive<StopSimulation>(m =>
            {
                StopSimulation();
            });
            Receive<ResumeSimulation>(m =>
            {
                _parent = Context.Sender;
                _projectId = m.ProjectId;
                Console.WriteLine($"Actor in path:{Context.Self.Path} - resuming simulation");
                Become(WaitingForSimulationToFinish);
            });
        }
        private void WaitingForSimulationToFinish()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Delay(5000, _cancellationTokenSource.Token)
                    .ContinueWith(_ => new SimulationFinished(_projectId), _cancellationTokenSource.Token)
                    .PipeTo(Self);
            Receive<SimulationFinished>(m =>
            {
                Console.WriteLine($"Actor in path:{Context.Self.Path} - simulation finished");
                _parent.Tell(new SimulationFinished(m.ProjectId), Self);
                Context.Self.Tell(PoisonPill.Instance);
            });
            Receive<StopSimulation>(m =>
            {
                StopSimulation();
            });
            Receive<PauseSimulation>(m =>
            {
                PauseSimulation();
            });

        }
        private void Paused()
        {
            Receive<StopSimulation>(m =>
            {
                StopSimulation();
            });
            Receive<ResumeSimulation>(m =>
            {
                ResumeSimulation();
            });
        }
        private void StopSimulation()
        {
            Console.WriteLine($"Actor in path:{Context.Self.Path} - stopping simulation");

            _cancellationTokenSource.Cancel();
            _parent.Tell(new SimulationStopped { ProjectId = _projectId });
            Context.Self.Tell(PoisonPill.Instance);
        }
        private void PauseSimulation()
        {
            Console.WriteLine($"Actor in path:{Context.Self.Path} - pausing simulation");

            _cancellationTokenSource.Cancel();
            _parent.Tell(new SimulationPaused { ProjectId = _projectId });
            Become(Paused);
        }
        private void ResumeSimulation()
        {
            Console.WriteLine($"Actor in path:{Context.Self.Path} - resuming simulation");
            Become(WaitingForSimulationToFinish);
        }


        protected override void PostStop()
        {
            base.PostStop();
            Console.WriteLine($"Actor in path:{Context.Self.Path} - stopped");
        }
    }
}