using System;

namespace Simulator.Messages.Messages
{
    public class SimulationFinished
    {
        public SimulationFinished()
        {
            
        }
        public SimulationFinished(Guid projectId)
        {
            ProjectId = projectId;
        }

        public Guid ProjectId { get; set; }
    }
}