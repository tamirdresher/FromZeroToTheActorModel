using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Simulator.Messages;
using Simulator.Messages.Messages;

namespace Simulator.FrontEnd.Models
{
    public class Simulation
    {
        public Guid ProjectId { get; set; }
        public SimulationState SimulationState { get; set; }
        public Technology TechnologyType { get; set; }
    }
}
