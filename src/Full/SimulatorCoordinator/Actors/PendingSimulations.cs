using System;
using Simulator.Messages;
using Simulator.Messages.Messages;

namespace Simulator.Coordinator.Actors
{
    internal class PendingSimulations
    {
        public Guid ProjectId { get; set; }
        public Technology Technology { get; set; }
        public SimulationState SimulationState { get; set; }
    }
}