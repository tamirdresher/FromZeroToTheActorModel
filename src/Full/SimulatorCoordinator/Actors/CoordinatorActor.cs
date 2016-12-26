using System;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using Simulator.Messages;
using Simulator.Messages.Messages;
using Simulator.Messages.Messages.SimulationQueue;

namespace Simulator.Coordinator.Actors
{
    public class CoordinatorActor : ReceiveActor
    {
        private readonly ISystemActors _systemActors;

        private IActorRef _queue;
        Guid _currentlySimulating = Guid.Empty;

       
        public CoordinatorActor(ISystemActors systemActors)
        {
            _systemActors = systemActors;
            _queue = Context.ActorOf(Props.Create(() => new SimulationQueueActor(_systemActors)));

            Become(FreeForSimulations);
        }

        private void FreeForSimulations()
        {
            ReceiveExternalRequests();

            Receive<SimulationStateChanged>(simState =>
            {
                _queue.Tell(new GetNextPending());
            });
            Receive<NextPending>(next =>
            {
                Become(() => Simulating(next));
            });

            //when we're free, we can accept what's waiting in the queue
            _queue.Tell(new GetNextPending());
        }

        private void Simulating(NextPending next)
        {
            _currentlySimulating = next.QueueEntry.ProjId;
            _systemActors.TechnologyCoordinators.ForEach(
                tech =>
                {
                    tech.Tell(new StartSimulation
                    {
                        ProjectId = next.QueueEntry.ProjId,
                        Technology = next.QueueEntry.Technology
                    });
                    if (next.QueueEntry.SimulationState == SimulationState.WaitingToStop)
                    {
                        tech.Tell(new StopSimulation{ProjectId = next.QueueEntry.ProjId});
                    }
                });

            if (next.QueueEntry.SimulationState==SimulationState.WaitingToStop)
            {
                _queue.Tell(new MoveToStopped {ProjectId = next.QueueEntry.ProjId});
            }
            if (next.QueueEntry.SimulationState == SimulationState.WaitingToPause)
            {
                _queue.Tell(new MoveToPaused {ProjectId = next.QueueEntry.ProjId});
            }
            else
            {
            _queue.Tell(new MoveToRunning { ProjectId = next.QueueEntry.ProjId });

            }

            ReceiveExternalRequests();

            Receive<SimulationStateChanged>(simState => simState.ProjectId == _currentlySimulating,
                simState =>
                {
                    switch (simState.SimulationState)
                    {
                        case SimulationState.Completed:
                        case SimulationState.Paused:
                        case SimulationState.Stopped:
                            {
                                _currentlySimulating = Guid.Empty;
                                Become(FreeForSimulations);
                                break;
                            }
                        case SimulationState.WaitingToResume:
                            {
                                //in order to not be dependent on the technologies, we broadcast to all technologies.
                                //the technology that this project belong to will react 
                                _systemActors.TechnologyCoordinators.ForEach(tech => tech.Tell(new ResumeSimulation{ ProjectId = simState.ProjectId }));
                                _queue.Tell(new MoveToRunning { OriginalSender = Sender, ProjectId = simState.ProjectId });
                                break;

                            }
                        case SimulationState.WaitingToStop:
                            {
                                //in order to not be dependent on the technologies, we broadcast to all technologies.
                                //the technology that this project belong to will react 
                                _systemActors.TechnologyCoordinators.ForEach(tech=>tech.Tell(new StopSimulation { ProjectId = simState.ProjectId }));
                                _queue.Tell(new MoveToStopped { OriginalSender = Sender, ProjectId = simState.ProjectId });
                                break;

                            }
                        case SimulationState.WaitingToPause:
                            {
                                //in order to not be dependent on the technologies, we broadcast to all technologies.
                                //the technology that this project belong to will react 
                                _systemActors.TechnologyCoordinators.ForEach(tech => tech.Tell(new PauseSimulation { ProjectId = simState.ProjectId }));
                                _queue.Tell(new MoveToPaused() { OriginalSender = Sender, ProjectId = simState.ProjectId });
                                break;
                            }
                    }
                });
            Receive<SimulationFinished>(m => m.ProjectId == _currentlySimulating,
                m =>
                {
                    _queue.Tell(new MoveToCompleted { OriginalSender = Sender, ProjectId = m.ProjectId });
                });
        }

        private void ReceiveExternalRequests()
        {
            Receive<GetQueue>(getQueue => { Context.GetLogger().Info("GetQueue sender:"+Sender); _queue.Forward(getQueue); });
            Receive<StartSimulation>(m => { Context.GetLogger().Info("StartSimulation sender:" + Sender); AddSimulation(m); });
            Receive<StopSimulation>(stop =>
            {
                _queue.Tell(new MoveToStopping { OriginalSender = Sender, ProjectId = stop.ProjectId });
            });
            Receive<PauseSimulation>(pauseSimulation =>
            {
                _queue.Tell(new MoveToPausing { OriginalSender = Sender, ProjectId = pauseSimulation.ProjectId });
            });
            Receive<ResumeSimulation>(resumeSimulation =>
            {
                _queue.Tell(new MoveToResume { OriginalSender = Sender, ProjectId = resumeSimulation.ProjectId });
            });
        }

        private void AddSimulation(StartSimulation m)
        {
            Context.GetLogger().Info($"Coordinator - Starting simulation of {m.ProjectId}");
            _queue.Tell(new AddNewSimulation() { OriginalSender = Sender, ProjId = m.ProjectId, Technology = m.Technology });
        }
        protected override void PreStart()
        {
            base.PreStart();
            _queue.Tell(new GetNextPending());
        }
    }
}