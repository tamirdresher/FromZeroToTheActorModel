using Akka.Actor;
using Simulator.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Akka.TestKit;
using FakeItEasy;
using Simulator.Coordinator;
using Simulator.Coordinator.Actors;
using Simulator.Messages.Messages;
using Simulator.Messages.Messages.SimulationQueue;

namespace Simulator.Tests
{

    [TestClass]
    public class CoordinatorTests : Akka.TestKit.VsTest.TestKit
    {
        private TestProbe _simulationNotifier;

        private IActorRef CreateCoordinator()
        {
            var props = CreateCoordinatorProps();
            var coordinator = this.Sys.ActorOf(props);
            return coordinator;
        }

        private Props CreateCoordinatorProps()
        {
            _simulationNotifier = CreateTestProbe(Sys);
            var systemActors = A.Fake<ISystemActors>();
            A.CallTo(() => systemActors.SimulationNotifier).Returns(_simulationNotifier);
            var props = Props.Create(() => new CoordinatorActor(systemActors));
            return props;
        }

        [TestMethod]
        public async Task StartSimulation_QueueEmpty_SimulationStarted()
        {
            var projId = Guid.NewGuid();
            var props = CreateCoordinatorProps();

            //this allows to control the parent of the actor            
            var coordinator = ActorOfAsTestActorRef<CoordinatorActor>(props, TestActor);
            coordinator.Tell(new StartSimulation() { ProjectId = projId, Technology = Technology.TechnologyA });

            _simulationNotifier.FishForMessage<SimulationStateChanged>(m=> m.SimulationState==SimulationState.Running);

            var queueEntries = await coordinator.Ask<SimulationQueueEntries>(new GetQueue());

            Assert.AreEqual(SimulationState.Running, queueEntries.Entries[0].SimulationState);
        }

        [TestMethod]
        public async Task StopSimulation_RunningSimulation_SimulationWaitingToStop()
        {
            var projId = Guid.NewGuid();
            var coordinator = CreateCoordinator();

            await coordinator.Ask(new StartSimulation() { ProjectId = projId, Technology = Technology.TechnologyA });
            var simState=await coordinator.Ask<SimulationStateChanged>(new StopSimulation() { ProjectId = projId});

            Assert.AreEqual(SimulationState.WaitingToStop, simState.SimulationState);
        }

        [TestMethod]
        public async Task StopSimulation_PausedSimulation_SimulationStopped()
        {
            var projId = Guid.NewGuid();
            var coordinator = CreateCoordinator();

            await coordinator.Ask(new StartSimulation() { ProjectId = projId, Technology = Technology.TechnologyA });

            //this is temp workaround to wait for the simulation to begin 
            await coordinator.Ask(new GetQueue());

            await coordinator.Ask<SimulationStateChanged>(new PauseSimulation() { ProjectId = projId });
            //var simState=await coordinator.Ask<SimulationStateChanged>(new StopSimulation() { ProjectId = projId });
            coordinator.Tell(new StopSimulation() { ProjectId = projId });
            var simState = FishForMessage<SimulationStateChanged>(m => m.SimulationState == SimulationState.WaitingToStop);
            Assert.AreEqual(SimulationState.WaitingToStop, simState.SimulationState);
        }

       

        [TestMethod]
        public async Task StopSimulation_PendingSimulation_SimulationStopped()
        {
            var projId = Guid.NewGuid();
            var proj2Id = Guid.NewGuid();
            var coordinator = CreateCoordinator();

            await coordinator.Ask(new StartSimulation { ProjectId = projId, Technology = Technology.TechnologyA });
            await coordinator.Ask(new StartSimulation { ProjectId = proj2Id, Technology = Technology.TechnologyA });
            var simState = await coordinator.Ask<SimulationStateChanged>(new StopSimulation() { ProjectId = proj2Id });

            Assert.AreEqual(SimulationState.Stopped, simState.SimulationState);
        }
    }
}