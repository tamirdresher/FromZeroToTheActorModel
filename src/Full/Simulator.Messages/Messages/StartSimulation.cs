namespace Simulator.Messages.Messages
{
    public class StartSimulation: ProjectSimulationMessage,ITechnologyAwareMessage
    {
        public Technology Technology { get; set; }
    }
    
}