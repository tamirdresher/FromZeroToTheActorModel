using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using Simulator.Messages.Messages.SimulationQueue;

namespace SimulatorCoordintaor.SignalR.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:8083";
            using (WebApp.Start(url))
            {
                Console.WriteLine("SignalR server running on {0}", url);
                Console.ReadLine();
            }
        }
    }
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();

        }
    }
    public class SimulationNotificationHub : Hub
    {
        public void Notify(SimulationStateChanged changeNotification)
        {
            Clients.All.SimulationStateChanged(changeNotification);
        }
    }
}

