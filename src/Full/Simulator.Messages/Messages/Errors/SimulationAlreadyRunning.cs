using System;

namespace Simulator.Messages.Messages.Errors
{
    public class SimulationAlreadyRunning
    {
        public Guid ProjId { get; set; }
    }

    public class SimulationStateChangeIsInvalid
    {
        public Guid ProjId { get; set; }
        public SimulationState CurrentState { get; set; }
        public SimulationState RequestedState { get; set; }
    }
}