using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Simulator.Messages;
using Simulator.Messages.Messages;
using Simulator.Messages.Messages.Errors;
using Simulator.Messages.Messages.SimulationQueue;

namespace Simulator.Coordinator.Actors
{
    public class SimulationQueueActor : ReceiveActor
    {
        private readonly ISystemActors _systemActors;
        List<SimulationItem> _simulationQueue = new List<SimulationItem>();
        public SimulationQueueActor(ISystemActors systemActors)
        {
            _systemActors = systemActors;
            ConfigureReceives();
        }

        private void ConfigureReceives()
        {
            Receive<AddNewSimulation>(sim => AddNewSimulation(sim));
            Receive<MoveToPausing>(sim => MoveToPausing(sim));
            Receive<MoveToPaused>(sim => MoveToPaused(sim));
            Receive<MoveToStopping>(sim => MoveToStopping(sim));
            Receive<MoveToResume>(sim => MoveToResume(sim));
            Receive<MoveToRunning>(sim => MoveToRunning(sim));
            Receive<GetQueue>(_ => GetQueue());
            Receive<GetNextPending>(_ => GetNextPending());
            Receive<MoveToStopped>(toStop => MoveToStopped(toStop));
            Receive<MoveToCompleted>(toComplete => MoveToCompleted(toComplete));
        }

        private void MoveToStopped(MoveToStopped toStop)
        {
            var simulation = _simulationQueue.FirstOrDefault(s => s.ProjectId == toStop.ProjectId);
            if (simulation == null)
            {
                Sender.Tell(new SimulationNotFound() { ProjId = toStop.ProjectId });
                return;
            }

            _simulationQueue.Remove(simulation);
            var simulationStateChanged = new SimulationStateChanged()
            {
                ProjectId = toStop.ProjectId,
                SimulationState = SimulationState.Stopped,
                Technology = simulation.Technology
            };
            NotifyAll(simulationStateChanged, Context.Parent);
        }
        private void GetNextPending()
        {
            var pendingSimulation =
                _simulationQueue.FirstOrDefault(x => x.SimulationState == SimulationState.Running) ??
                _simulationQueue.FirstOrDefault(x => x.SimulationState == SimulationState.WaitingToStop) ??
                _simulationQueue.FirstOrDefault(x => x.SimulationState == SimulationState.WaitingToPause) ??
                _simulationQueue.FirstOrDefault(x => x.SimulationState == SimulationState.WaitingToResume) ??
                _simulationQueue.FirstOrDefault(x => x.SimulationState == SimulationState.Waiting);

            if (pendingSimulation != null)
            {
                Sender.Tell(new NextPending
                {
                    QueueEntry =
                        new QueueEntry()
                        {
                            ProjId = pendingSimulation.ProjectId,
                            Technology = pendingSimulation.Technology,
                            SimulationState = pendingSimulation.SimulationState
                        }
                });
            }
        }
        private void GetQueue()
        {
            Sender.Tell(new SimulationQueueEntries()
            {
                Entries =
                    _simulationQueue.Select(
                        x =>
                            new QueueEntry()
                            {
                                ProjId = x.ProjectId,
                                Technology = x.Technology,
                                SimulationState = x.SimulationState
                            }).ToArray()
            });
        }
        private void MoveToRunning(MoveToRunning sim)
        {
            var simulation = _simulationQueue.FirstOrDefault(s => s.ProjectId == sim.ProjectId);
            if (simulation == null)
            {
                Context.Sender.Tell(new SimulationNotFound { ProjId = sim.ProjectId });
                return;
            }

            simulation.SimulationState = SimulationState.Running;
            var simulationStateChanged = new SimulationStateChanged()
            {
                ProjectId = sim.ProjectId,
                SimulationState = simulation.SimulationState,
                Technology = simulation.Technology
            };

            NotifyAll(simulationStateChanged, Context.Sender, Context.Parent);
        }
        private void MoveToResume(MoveToResume sim)
        {
            var simulation = _simulationQueue.FirstOrDefault(s => s.ProjectId == sim.ProjectId);
            if (simulation == null)
            {
                sim.OriginalSender.Tell(new SimulationNotFound() { ProjId = sim.ProjectId });
                return;
            }
            if (simulation.SimulationState != SimulationState.Paused)
            {
                sim.OriginalSender.Tell(new SimulationStateChangeIsInvalid()
                {
                    ProjId = sim.ProjectId,
                    CurrentState = simulation.SimulationState,
                    RequestedState = SimulationState.WaitingToPause
                });
                return;
            }


            simulation.SimulationState = SimulationState.WaitingToResume;

            var simulationStateChanged = new SimulationStateChanged()
            {
                ProjectId = sim.ProjectId,
                SimulationState = simulation.SimulationState,
                Technology = simulation.Technology
            };

            NotifyAll(simulationStateChanged, sim.OriginalSender, Context.Parent);
        }
        private void MoveToStopping(MoveToStopping sim)
        {
            var simulation = _simulationQueue.FirstOrDefault(s => s.ProjectId == sim.ProjectId);
            if (simulation == null)
            {
                sim.OriginalSender.Tell(new SimulationNotFound() { ProjId = sim.ProjectId });
                return;
            }
            if (simulation.SimulationState == SimulationState.WaitingToPause ||
                simulation.SimulationState == SimulationState.WaitingToStop)
            {
                sim.OriginalSender.Tell(new SimulationStateChangeIsInvalid()
                {
                    ProjId = sim.ProjectId,
                    CurrentState = simulation.SimulationState,
                    RequestedState = SimulationState.WaitingToStop
                });
                return;
            }

            if (simulation.SimulationState == SimulationState.Waiting)
            {
                _simulationQueue.Remove(simulation);
                var simulationStateChangedToStopped = new SimulationStateChanged()
                {
                    ProjectId = sim.ProjectId,
                    SimulationState = SimulationState.Stopped,
                    Technology = simulation.Technology
                };
                sim.OriginalSender.Tell(simulationStateChangedToStopped);
                Context.Parent.Tell(simulationStateChangedToStopped);
                return;
            }
            simulation.SimulationState = SimulationState.WaitingToStop;
            var simulationStateChanged = new SimulationStateChanged()
            {
                ProjectId = sim.ProjectId,
                SimulationState = simulation.SimulationState,
                Technology = simulation.Technology
            };

            sim.OriginalSender.Tell(simulationStateChanged);
            Context.Parent.Tell(simulationStateChanged);
        }
        private void MoveToPaused(MoveToPaused sim)
        {
            var simulation = _simulationQueue.FirstOrDefault(s => s.ProjectId == sim.ProjectId);
            if (simulation == null)
            {
                sim.OriginalSender.Tell(new SimulationNotFound() { ProjId = sim.ProjectId });
                return;
            }
            if (simulation.SimulationState != SimulationState.WaitingToPause)
            {
                sim.OriginalSender.Tell(new SimulationStateChangeIsInvalid()
                {
                    ProjId = sim.ProjectId,
                    CurrentState = simulation.SimulationState,
                    RequestedState = SimulationState.Paused
                });
                return;
            }

            simulation.SimulationState = SimulationState.Paused;
            var simulationStateChanged = new SimulationStateChanged()
            {
                ProjectId = sim.ProjectId,
                SimulationState = simulation.SimulationState,
                Technology = simulation.Technology
            };

            NotifyAll(simulationStateChanged, sim.OriginalSender, Context.Parent);
        }
        private void MoveToPausing(MoveToPausing sim)
        {
            var simulation = _simulationQueue.FirstOrDefault(s => s.ProjectId == sim.ProjectId);
            if (simulation == null)
            {
                sim.OriginalSender.Tell(new SimulationNotFound() { ProjId = sim.ProjectId });
                return;
            }
            if (simulation.SimulationState == SimulationState.Waiting ||
                simulation.SimulationState == SimulationState.WaitingToResume)
            {
                simulation.SimulationState = SimulationState.Paused;
            }
            else if (simulation.SimulationState != SimulationState.Running)
            {
                sim.OriginalSender.Tell(new SimulationStateChangeIsInvalid()
                {
                    ProjId = sim.ProjectId,
                    CurrentState = simulation.SimulationState,
                    RequestedState = SimulationState.WaitingToPause
                });
                return;
            }
            else
            {
                simulation.SimulationState = SimulationState.WaitingToPause;
            }

            var simulationStateChanged = new SimulationStateChanged()
            {
                ProjectId = sim.ProjectId,
                SimulationState = simulation.SimulationState,
                Technology = simulation.Technology
            };

            NotifyAll(simulationStateChanged, sim.OriginalSender, Context.Parent);
        }
        private void MoveToCompleted(MoveToCompleted toComplete)
        {
            var simulation = _simulationQueue.FirstOrDefault(s => s.ProjectId == toComplete.ProjectId);
            if (simulation == null)
            {
                Sender.Tell(new SimulationNotFound() { ProjId = toComplete.ProjectId });
                return;
            }

            _simulationQueue.Remove(simulation);
            var simulationStateChanged = new SimulationStateChanged()
            {
                ProjectId = toComplete.ProjectId,
                SimulationState = SimulationState.Completed,
                Technology = simulation.Technology
            };
            NotifyAll(simulationStateChanged, Context.Parent);
        }

        private void AddNewSimulation(AddNewSimulation sim)
        {
            if (_simulationQueue.Any(s => s.ProjectId == sim.ProjId))
            {
                sim.OriginalSender.Tell(new SimulationAlreadyRunning { ProjId = sim.ProjId });
                return;
            }

            _simulationQueue.Add(new SimulationItem
            {
                ProjectId = sim.ProjId,
                Technology = sim.Technology,
                SimulationState = SimulationState.Waiting
            });
            var simulationStateChanged = new SimulationStateChanged
            {
                ProjectId = sim.ProjId,
                SimulationState = SimulationState.Waiting,
                Technology = sim.Technology
            };
            NotifyAll(simulationStateChanged, sim.OriginalSender, Context.Parent);
        }
        private void NotifyAll(SimulationStateChanged simulationStateChanged, params IActorRef[] receivers)
        {
            foreach (var receiver in receivers)
            {
                receiver.Tell(simulationStateChanged);
            }
            _systemActors.SimulationNotifier.Tell(simulationStateChanged);
        }
    }



    internal class SimulationItem
    {
        public Guid ProjectId { get; set; }
        public SimulationState SimulationState { get; set; }
        public Technology Technology { get; set; }
    }
}