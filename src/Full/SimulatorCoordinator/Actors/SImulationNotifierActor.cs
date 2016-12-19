using Akka.Actor;
using Akka.Event;
using Microsoft.AspNet.SignalR.Client;
using Simulator.Messages.Messages.SimulationQueue;

namespace Simulator.Coordinator.Actors
{
    class SImulationNotifierActor:ReceiveActor
    {
        private IHubProxy _simulationNotificationHub;

        public SImulationNotifierActor()
        {
            var hubConnection = new HubConnection("http://localhost:8083/signalr");
            _simulationNotificationHub = hubConnection.CreateHubProxy("SimulationNotificationHub");
            hubConnection.Start().Wait();
            Become(Listening);
        }

        private void Listening()
        {
            Receive<SimulationStateChanged>(m =>
            {
                Context.GetLogger().Info($"Simulation State Changed. ProjectId: {m.ProjectId}, State: {m.SimulationState}");
                _simulationNotificationHub.Invoke("Notify",m);
            });
            ReceiveAny(m =>
            {
                Context.GetLogger().Warning($"SImulationNotifier - Unknown message received. {m.ToString()}");

            });
        }

        
    }
}
