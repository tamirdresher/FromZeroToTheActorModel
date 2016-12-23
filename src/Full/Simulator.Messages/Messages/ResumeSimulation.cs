namespace Simulator.Messages.Messages
{
    public class ResumeSimulation: ProjectSimulationMessage,ITechnologyAwareMessage
    {
        public Technology Technology { get; set; }
    }
}