using System;
using Simulator.Messages;

namespace Simulator.Coordinator.Actors
{
    class ProjectSimulationUnitActor
    {

        public ProjectSimulationUnitActor(Guid projectId)
        {
            ProjectId = projectId;
            SimulationState=SimulationState.Waiting;
        }
        public Guid ProjectId { get; set; }
        public SimulationState SimulationState { get; set; }
    }
}