using System;

namespace Simulator.Messages.Messages
{
    public class SimulationFinished
    {
        public SimulationFinished()
        {
            
        }
        public SimulationFinished(Guid projId)
        {
            ProjId = projId;
        }

        public Guid ProjId { get; set; }
    }
}