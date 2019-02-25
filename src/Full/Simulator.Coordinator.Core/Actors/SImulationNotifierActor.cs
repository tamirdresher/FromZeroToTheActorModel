using Akka.Actor;
using Akka.Event;
using Microsoft.AspNet.SignalR.Client;
using Simulator.Messages.Messages.SimulationQueue;
using System;

namespace Simulator.Coordinator.Actors
{
    class SimulationNotifierActor:ReceiveActor
    {
        private Lazy<IHubProxy> _simulationNotificationHub;

        public SimulationNotifierActor()
        {
            _simulationNotificationHub = new Lazy<IHubProxy>(() =>
            {
                var hubConnection = new HubConnection("http://localhost:8083/signalr");
                var simulationNotificationHub = hubConnection.CreateHubProxy("SimulationNotificationHub");
                hubConnection.Start().Wait();
                return simulationNotificationHub;
            });
            Become(Listening);
        }
        IHubProxy SimulationNotificationHub => _simulationNotificationHub.Value;
        private void Listening()
        {
            Receive<SimulationStateChanged>(m =>
            {
                Context.GetLogger().Info($"Simulation State Changed. ProjectId: {m.ProjectId}, State: {m.SimulationState}");
                SimulationNotificationHub.Invoke("Notify",m);
            });
            ReceiveAny(m =>
            {
                Context.GetLogger().Warning($"SImulationNotifier - Unknown message received. {m.ToString()}");

            });
        }

        
    }
}
