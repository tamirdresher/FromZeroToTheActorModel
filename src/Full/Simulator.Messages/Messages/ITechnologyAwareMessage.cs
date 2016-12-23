namespace Simulator.Messages.Messages
{
    public interface ITechnologyAwareMessage
    {
        Technology Technology { get; }
    }
}