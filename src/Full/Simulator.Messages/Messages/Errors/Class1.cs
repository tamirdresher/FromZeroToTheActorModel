namespace Simulator.Messages.Messages.Errors
{
    public class InvalidSimualtionRequest
    {
        public string Error { get; }

        public InvalidSimualtionRequest(string error)
        {
            Error = error;
        }
    }
}
