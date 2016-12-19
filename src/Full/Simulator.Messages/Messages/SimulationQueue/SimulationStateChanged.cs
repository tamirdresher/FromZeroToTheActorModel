using System;

namespace Simulator.Messages.Messages.SimulationQueue
{
    public class SimulationStateChanged
    {
        public Guid ProjectId { get; set; }
        public SimulationState SimulationState { get; set; }
        public Technology Technology { get; set; }
    }
}
