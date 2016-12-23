using Akka.Actor;
using Akka.Event;
using Microsoft.AspNet.SignalR.Client;
using Simulator.Messages.Messages.SimulationQueue;

namespace Simulator.Coordinator.Actors
{
    class SimulationNotifierActor:ReceiveActor
    {
        private IHubProxy _simulationNotificationHub;

        public SimulationNotifierActor()
        {
            var hubConnection = new HubConnection("http://localhost:8083/signalr");
            _simulationNotificationHub = hubConnection.CreateHubProxy("SimulationNotificationHub");
            hubConnection.Start().Wait(1000);
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
