namespace Simulator.Messages
{
    public enum SimulationState
    {
        Running,
        Waiting,
        WaitingToStop,
        WaitingToPause,
        WaitingToResume,
        Paused,
        Stopped,
        Completed,
    }
}