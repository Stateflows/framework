using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal sealed class Executor : IDisposable
    {
        public readonly Graph Graph;

        public StateMachinesRegister Register { get; set; }

        public IServiceProvider ServiceProvider => Scope.ServiceProvider;

        private readonly IServiceScope Scope;

        public Executor(StateMachinesRegister register, Graph graph, IServiceProvider serviceProvider, StateflowsContext stateflowsContext, Event @event)
        {
            Register = register;
            Scope = serviceProvider.CreateScope();
            Graph = graph;
            Context = new RootContext(stateflowsContext, this, @event);
            var logger = ServiceProvider.GetService<ILogger<Executor>>();
            Inspector = new Inspector(this, logger);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }

        public readonly RootContext Context;

        public readonly Inspector Inspector;

        public IEnumerable<string> GetDeferredEvents()
            => VerticesStack.SelectMany(vertex => vertex.DeferredEvents).ToArray();

        public IEnumerable<Vertex> VerticesStack { get; private set; } = null;

        public void RebuildVerticesStack()
        {
            var result = new List<Vertex>();
            for (int i = 0; i < Context.StatesStack.Count; i++)
            {
                var vertexName = Context.StatesStack[i];
                if (Graph.AllVertices.TryGetValue(vertexName, out var vertex))
                {
                    result.Add(vertex);
                }
                else
                {
                    while (Context.StatesStack.Count > i)
                    {
                        Context.StatesStack.RemoveAt(i);
                    }

                    break;
                }
            }

            VerticesStack = result;
        }

        public IEnumerable<string> GetStateStack()
            => VerticesStack.Select(vertex => vertex.Name).ToArray();

        public Task HydrateAsync()
            => Inspector.AfterHydrateAsync(new StateMachineActionContext(Context));

        public Task DehydrateAsync()
            => Inspector.BeforeDehydrateAsync(new StateMachineActionContext(Context));

        public bool Initialized
            => VerticesStack.Any();

        public bool Finalized
            =>
                VerticesStack.Count() == 1 &&
                VerticesStack.First().Type == VertexType.FinalState;

        public BehaviorStatus BehaviorStatus =>
            (Initialized, Finalized) switch
            {
                (false, false) => BehaviorStatus.NotInitialized,
                (true, false) => BehaviorStatus.Initialized,
                (true, true) => BehaviorStatus.Finalized,
                _ => BehaviorStatus.NotInitialized
            };

        public async Task<bool> InitializeAsync(InitializationRequest @event)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (!Initialized && await DoInitializeStateMachineAsync(@event))
            {
                await DoInitializeCascadeAsync(Graph.InitialVertex);

                await DoCompletion();

                return true;
            }

            return false;
        }

        public async Task ExitAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                foreach (var vertex in VerticesStack)
                {
                    await DoExitAsync(vertex);
                }

                await DoFinalizeStateMachineAsync();

                Context.StatesStack.Clear();
            }
        }

        public void Reset()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                Context.Context.Values.Clear();
                Context.Context.Version = 0;
            }
        }

        private async Task DoInitializeCascadeAsync(Vertex vertex)
        {
            while (vertex != null)
            {
                if (vertex.Parent != null)
                {
                    await DoInitializeStateAsync(vertex.Parent);
                }

                await DoEntryAsync(vertex);
                Context.StatesStack.Add(vertex.Identifier);

                vertex = vertex.InitialVertex;
            }
        }

        private async Task DoInitializeCascadeAsync2(Vertex vertex)
        {
            while (vertex != null)
            {
                if (vertex.InitialVertex != null)
                {
                    await DoEntryAsync(vertex.InitialVertex);

                    if (vertex.InitialVertex.InitialVertex != null)
                    {
                        await DoInitializeStateAsync(vertex.InitialVertex);
                    }
                }

                Context.StatesStack.Add(vertex.Identifier);

                vertex = vertex.InitialVertex;
            }
        }

        public IEnumerable<Type> GetExpectedEvents()
        {
            var currentStack = VerticesStack.ToList();
            currentStack.Reverse();

            if (currentStack.Any())
            {
                return currentStack
                    .SelectMany(vertex => vertex.Edges.Values)
                    .Select(edge => edge.TriggerType)
                    .Distinct();
            }
            else
            {
                return Graph.InitializerTypes.Any()
                    ? Graph.InitializerTypes
                    : new List<Type>() { typeof(InitializationRequest) };
            }
        }

        private List<Vertex> GetNestedVertices(Vertex vertex)
        {
            var result = new List<Vertex>() { vertex };
            foreach (var childVertex in vertex.Vertices.Values)
            {
                result.AddRange(GetNestedVertices(childVertex));
            }

            return result;
        }

        public async Task<EventStatus> ProcessAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var result = EventStatus.Rejected;

            if (Initialized)
            {
                result = await DoProcessAsync(@event);
            }

            return result;
        }

        private bool TryDeferEvent<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            var deferredEvents = GetDeferredEvents();
            if (deferredEvents.Any() && deferredEvents.Contains(@event.Name))
            {
                Context.DeferredEvents.Add(@event);
                return true;
            }
            return false;
        }

        private async Task DispatchNextDeferredEvent()
        {
            var deferredEvents = GetDeferredEvents();
            foreach (var @event in Context.DeferredEvents)
            {
                if (!deferredEvents.Any() || !deferredEvents.Contains(@event.Name))
                {
                    Context.DeferredEvents.Remove(@event);

                    Context.SetEvent(@event);

                    RebuildVerticesStack();

                    await DoProcessAsync(@event);

                    Context.ClearEvent();

                    break;
                }
            }
        }

        private async Task<EventStatus> DoProcessAsync<TEvent>(TEvent @event)
            where TEvent : Event, new()
        {
            Debug.Assert(Context != null, $"Context is not available. Is state machine '{Graph.Name}' hydrated?");

            var currentStack = VerticesStack.ToList();
            currentStack.Reverse();

            var deferred = true;

            if (!TryDeferEvent(@event))
            {
                deferred = false;

                foreach (var vertex in currentStack)
                {
                    foreach (var edge in vertex.OrderedEdges)
                    {
                        Context.SourceState = edge.SourceName;
                        Context.TargetState = edge.TargetName;

                        if (@event.Triggers(edge) && await DoGuardAsync<TEvent>(edge))
                        {
                            await DoConsumeAsync<TEvent>(edge);

                            await DoCompletion();

                            return EventStatus.Consumed;
                        }
                    }
                }
            }

            if (!deferred)
            {
                RebuildVerticesStack();

                await DispatchNextDeferredEvent();
            }

            return deferred
                ? EventStatus.Deferred
                : EventStatus.NotConsumed;
        }

        [DebuggerStepThrough]
        private async Task<bool> DoGuardAsync<TEvent>(Edge edge)
            where TEvent : Event, new()
        {
            var context = new GuardContext<TEvent>(Context, edge);

            await Inspector.BeforeTransitionGuardAsync(context);

            var result = await edge.Guards.WhenAll(Context);

            await Inspector.AfterGuardAsync(context, result);

            return result;
        }

        private async Task DoEffectAsync<TEvent>(Edge edge)
            where TEvent : Event, new()
        {
            var context = new TransitionContext<TEvent>(Context, edge);

            await Inspector.BeforeEffectAsync(context);

            await edge.Effects.WhenAll(Context);

            await Inspector.AfterEffectAsync(context);
        }

        public async Task<bool> DoInitializeStateMachineAsync(InitializationRequest @event)
        {
            var result = false;

            if (
                Graph.Initializers.TryGetValue(@event.Name, out var initializer) ||
                (
                    @event.Name == EventInfo<InitializationRequest>.Name &&
                    !Graph.Initializers.Any()
                )
            )
            {
                var context = new StateMachineInitializationContext(Context, @event);
                await Inspector.BeforeStateMachineInitializeAsync(context);

                try
                {
                    result = (initializer == null) || await initializer.WhenAll(Context);
                }
                catch (Exception e)
                {
                    await Inspector.OnStateMachineInitializationExceptionAsync(context, e);

                    result = false;
                }

                await Inspector.AfterStateMachineInitializeAsync(context);
            }

            return result;
        }

        public async Task DoFinalizeStateMachineAsync()
        {
            var context = new StateMachineActionContext(Context);
            await Inspector.BeforeStateMachineFinalizeAsync(context);

            await Graph.Finalize.WhenAll(Context);

            await Inspector.AfterStateMachineFinalizeAsync(context);
        }

        public async Task DoInitializeStateAsync(Vertex vertex)
        {
            var context = new StateActionContext(Context, vertex, Constants.Initialize);
            await Inspector.BeforeStateInitializeAsync(context);

            await vertex.Initialize.WhenAll(Context);

            await Inspector.AfterStateInitializeAsync(context);
        }

        public async Task DoFinalizeStateAsync(Vertex vertex)
        {
            var context = new StateActionContext(Context, vertex, Constants.Finalize);
            await Inspector.BeforeStateFinalizeAsync(context);

            await vertex.Finalize.WhenAll(Context);

            await Inspector.AfterStateFinalizeAsync(context);
        }

        public async Task DoEntryAsync(Vertex vertex)
        {
            var context = new StateActionContext(Context, vertex, Constants.Entry);
            await Inspector.BeforeStateEntryAsync(context);

            await vertex.Entry.WhenAll(Context);

            await Inspector.AfterStateEntryAsync(context);
        }

        private async Task DoExitAsync(Vertex vertex)
        {
            var context = new StateActionContext(Context, vertex, Constants.Exit);
            await Inspector.BeforeStateExitAsync(context);

            await vertex.Exit.WhenAll(Context);

            await Inspector.AfterStateExitAsync(context);
        }

        private async Task DoConsumeAsync<TEvent>(Edge edge)
            where TEvent : Event, new()
        {
            var exitingVertices = new List<Vertex>();
            var enteringVertices = new List<Vertex>();

            var vrtx = VerticesStack.Last();
            while (vrtx != null)
            {
                exitingVertices.Insert(0, vrtx);
                vrtx = vrtx.Parent;
            }

            vrtx = edge.Target;
            while (vrtx != null)
            {
                enteringVertices.Insert(0, vrtx);
                vrtx = vrtx.Parent;
            }

            if (enteringVertices.Any() && exitingVertices.Any())
            {
                while (
                    enteringVertices.Any() &&
                    exitingVertices.Any() &&
                    enteringVertices[0] == exitingVertices[0] &&
                    enteringVertices[0] != edge.Target
                )
                {
                    enteringVertices.RemoveAt(0);
                    exitingVertices.RemoveAt(0);
                }

                exitingVertices.Reverse();
                foreach (var vertex in exitingVertices)
                {
                    await DoExitAsync(vertex);
                    Context.StatesStack.RemoveAt(Context.StatesStack.Count - 1);
                }
            }

            await DoEffectAsync<TEvent>(edge);

            foreach (var vertex in enteringVertices)
            {
                await DoEntryAsync(vertex);

                if (vertex.Vertices.Any())
                {
                    await DoInitializeStateAsync(vertex);
                }

                if (vertex != enteringVertices.Last())
                {
                    Context.StatesStack.Add(vertex.Identifier);
                }
            }

            if (edge.Target != null && enteringVertices.Any())
            {
                var topVertex = enteringVertices.Last();

                await DoInitializeCascadeAsync2(topVertex);

                if (topVertex.Type == VertexType.FinalState)
                {
                    if (topVertex.Parent is null)
                    {
                        await DoFinalizeStateMachineAsync();
                    }
                    else
                    {
                        await DoFinalizeStateAsync(topVertex.Parent);
                    }
                }
            }
        }

        private async Task DoCompletion()
        {
            Context.SetEvent(new CompletionEvent());

            RebuildVerticesStack();

            await DoProcessAsync(Context.Event);

            Context.ClearEvent();
        }

        public StateMachine GetStateMachine(Type stateMachineType, RootContext context)
        {
            var stateMachine = ServiceProvider.GetService(stateMachineType) as StateMachine;
            stateMachine.Context = new StateMachineActionContext(context);

            return stateMachine;
        }

        public TState GetState<TState>(IStateActionContext context)
            where TState : BaseState
        {
            var state = ServiceProvider.GetService<TState>();
            state.Context = context;
            
            return state;
        }

        public TTransition GetTransition<TTransition, TEvent>(ITransitionContext<TEvent> context)
            where TTransition : Transition<TEvent>
            where TEvent : Event, new()
        {
            var transition = ServiceProvider.GetService<TTransition>();
            transition.Context = context;
            
            return transition;
        }

        public TElseTransition GetElseTransition<TElseTransition, TEvent>(ITransitionContext<TEvent> context)
            where TElseTransition : ElseTransition<TEvent>
            where TEvent : Event, new()
        {
            var elseTransition = ServiceProvider.GetService<TElseTransition>();
            elseTransition.Context = context;
            
            return elseTransition;
        }
    }
}
