using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Simulator.Messages;
using Simulator.Messages.Messages;

namespace Simulator.Coordinator.Actors
{
    class ScolSimulationActor : ReceiveActor
    {
        private IActorRef _parent;
        private Guid _projectId;
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public ScolSimulationActor()
        {
            Become(ReadyToSimulate);
        }

        private void ReadyToSimulate()
        {
            Receive<StartSimulation>(m =>
            {
                _parent = Context.Sender;
                _projectId = m.ProjectId;
                Console.WriteLine($"Actor in path:{Context.Self.Path} - starting simulation");
                Task.Delay(5000, _cancellationTokenSource.Token)
                    .ContinueWith(_ => new SimulationFinished(_projectId), _cancellationTokenSource.Token)
                    .PipeTo(Self);
                Become(WaitingForSimulationToFinish);
            });
            Receive<StopSimulation>(m =>
            {
                StopSimulation();
            });
        }

        private void WaitingForSimulationToFinish()
        {
            Receive<SimulationFinished>(m =>
            {
                Console.WriteLine($"Actor in path:{Context.Self.Path} - simulation finished");
                _parent.Tell(new SimulationFinished(m.ProjId), Self);
            });
            Receive<StopSimulation>(m =>
            {

                StopSimulation();
            });
        }

        private void StopSimulation()
        {
            Console.WriteLine($"Actor in path:{Context.Self.Path} - stopping simulation");

            _cancellationTokenSource.Cancel();
            _parent.Tell(new SimulationStopped() {ProjectId = _projectId});
        }

        protected override void PostStop()
        {
            base.PostStop();
            Console.WriteLine($"Actor in path:{Context.Self.Path} - stopped");

        }
    }
}