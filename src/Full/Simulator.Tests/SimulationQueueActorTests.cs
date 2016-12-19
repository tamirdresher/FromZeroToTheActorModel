using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Akka.Actor;
using Akka.TestKit;
using Simulator.Messages;
using FakeItEasy;
using Simulator.Coordinator.Actors;
using Simulator.Messages.Messages;
using Simulator.Messages.Messages.Errors;
using Simulator.Messages.Messages.SimulationQueue;

namespace Simulator.Tests
{
    [TestClass]
    public class SimulationQueueActorTests: Akka.TestKit.VsTest.TestKit
    {
        private TestProbe _simulationNotifier;

        private IActorRef CreateQueueActor()
        {
            var props = CreateQueueActorProps();

            var queue = this.Sys.ActorOf(props);
            return queue;
        }

        private Props CreateQueueActorProps()
        {
            _simulationNotifier = CreateTestProbe(Sys);
            var systemActors = A.Fake<ISystemActors>();
            A.CallTo(() => systemActors.SimulationNotifier).Returns(_simulationNotifier);
            var props = Props.Create(() => new SimulationQueueActor(systemActors));
            return props;
        }

        [TestMethod]
        public void EmptyQueue_Should_Accpept_NewSimulation()
        {
            var projId = Guid.NewGuid();
            var props = CreateQueueActorProps();

            //this allows to control the parent of the actor            
            var queue = ActorOfAsTestActorRef<SimulationQueueActor>(props, TestActor);
            queue.Tell(new AddNewSimulation() { OriginalSender = this.TestActor, ProjId = projId, Technology = Technology.TechnologyA });

            var result = ExpectMsg<SimulationStateChanged>();

            Assert.AreEqual(SimulationState.Waiting, result.SimulationState);
        }

        [TestMethod]
        public void AddingTheSameProjectTwice_ShouldReturnAnError()
        {
            var projId = Guid.NewGuid();

            var queue = CreateQueueActor();
            queue.Tell(new AddNewSimulation() { OriginalSender = this.TestActor, ProjId = projId, Technology = Technology.TechnologyA });
            ExpectMsg<SimulationStateChanged>();

            queue.Tell(new AddNewSimulation() { OriginalSender = this.TestActor, ProjId = projId, Technology = Technology.TechnologyA });
            var result = ExpectMsg<SimulationAlreadyRunning>();

            Assert.AreEqual(projId, result.ProjId);
        }

        [TestMethod]
        public void QueueWithAnItem_Should_Accpept_NewSimulation()
        {
            var projId = Guid.NewGuid();
            var secondProjId = Guid.NewGuid();

            var queue = CreateQueueActor();
            queue.Tell(new AddNewSimulation() { OriginalSender = this.TestActor, ProjId = projId, Technology = Technology.TechnologyA });
            ExpectMsg<SimulationStateChanged>();

            queue.Tell(new AddNewSimulation() { OriginalSender = this.TestActor, ProjId = secondProjId, Technology = Technology.TechnologyA });            
            var result = ExpectMsg<SimulationStateChanged>();

            Assert.AreEqual(secondProjId, result.ProjectId);
            Assert.AreEqual(SimulationState.Waiting, result.SimulationState);
        }

       

        [TestMethod]
        public async Task GetQeueue_QueueWithTwoPending_TwoPendingReturns()
        {
            var projId = Guid.NewGuid();
            var secondProjId = Guid.NewGuid();
            var queue = CreateQueueActor();

            queue.Tell(new AddNewSimulation() { OriginalSender = this.TestActor, ProjId = projId, Technology = Technology.TechnologyA });
            ExpectMsg<SimulationStateChanged>();

            queue.Tell(new AddNewSimulation() { OriginalSender = this.TestActor, ProjId = secondProjId, Technology = Technology.TechnologyA });
            ExpectMsg<SimulationStateChanged>();

            var queueEntries = await queue.Ask<SimulationQueueEntries>(new GetQueue());
            
            Assert.AreEqual(2, queueEntries.Entries.Length);
            Assert.AreEqual(SimulationState.Waiting, queueEntries.Entries[0].SimulationState);
            Assert.AreEqual(SimulationState.Waiting, queueEntries.Entries[1].SimulationState);
        }
    }
}
