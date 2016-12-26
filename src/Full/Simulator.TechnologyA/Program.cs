using Akka.Actor;
using Simulator.TechnologyA.Actors;

namespace Simulator.TechnologyA
{
    class Program
    {
        private static ActorSystem _scolActorSystem;
        private static IActorRef _techACoordinator;

        static void Main(string[] args)
        {
            using (_scolActorSystem = ActorSystem.Create("TechnologyA"))
            {
                _techACoordinator = _scolActorSystem.ActorOf(Props.Create(() => new TechACoordinatorActor()), "coordinator");

                _scolActorSystem.WhenTerminated.Wait();
            }
        }
    }
}
