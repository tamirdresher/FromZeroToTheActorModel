using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Simulator.FrontEnd.Models;
using Simulator.Messages;
using Microsoft.AspNetCore.Mvc;
using Simulator.Messages.Messages;
using Simulator.Messages.Messages.SimulationQueue;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Simulator.FrontEnd.Controllers
{
    public class SimulationsController : Controller
    {
        
        public async Task<IActionResult> Index()
        {
            var coActorSelection = ActorSystemRefs.ActorSystem.ActorSelection("akka.tcp://simulator@localhost:8097/user/SimulationCoordinator");
            coActorSelection.Tell(new GetQueue());

            var queue =await ActorSystemRefs.Coordinator.Ask<SimulationQueueEntries>(new GetQueue(),TimeSpan.FromSeconds(10));
            return View(queue.Entries.Select(e => new Simulation
                {
                    ProjectId = e.ProjId,
                    SimulationState = e.SimulationState,
                    TechnologyType = e.Technology
                })
            );
        }

        [HttpPost]
        public IActionResult Pause(Guid id)
        {
            ActorSystemRefs.Coordinator.Tell(new PauseSimulation { ProjectId = id });

            return Ok();
        }

        [HttpPost]
        public IActionResult Resume(Guid id)
        {
            ActorSystemRefs.Coordinator.Tell(new ResumeSimulation{ProjectId = id });
            return Ok();
        }

        [HttpPost]
        public IActionResult Stop(Guid id)
        {
            ActorSystemRefs.Coordinator.Tell(new StopSimulation() { ProjectId = id });

            return Ok();
        }

        public async Task<IActionResult> Create()
        {
            ActorSystemRefs.Coordinator.Tell(new StartSimulation{ProjectId = Guid.NewGuid(),Technology = Technology.TechnologyA});
            return Ok();
        }
    }
}
