using System;

namespace Simulator.Messages.Messages
{
    public interface IProjectSimulationMessage
    {
        Guid ProjectId { get; set; }
    }

    public abstract class ProjectSimulationMessage : IProjectSimulationMessage
    {
        public Guid ProjectId { get; set; }
    }
}
