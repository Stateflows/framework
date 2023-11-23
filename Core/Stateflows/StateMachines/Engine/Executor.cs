using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal sealed class Executor : IDisposable
    {
        public Graph Graph { get; }

        public StateMachinesRegister Register { get; }

        public IServiceProvider ServiceProvider => Scope.ServiceProvider;

        private IServiceScope Scope { get; }

        public Executor(StateMachinesRegister register, Graph graph, IServiceProvider serviceProvider)
        {
            Register = register;
            Scope = serviceProvider.CreateScope();
            Graph = graph;
        }

        public RootContext Context { get; private set; }

        private Inspector inspector;

        [DebuggerHidden]
        public Inspector Inspector
            => inspector ??= new Inspector(this);

        public IEnumerable<string> GetDeferredEvents()
        {
            var stack = GetVerticesStack();
            return stack.SelectMany(vertex => vertex.DeferredEvents);
        }

        private List<Vertex> verticesStack = null;

        public List<Vertex> GetVerticesStack(bool forceRebuild = false)
        {
            if (verticesStack == null || forceRebuild)
            {
                Debug.Assert(Context != null, $"Context is not available. Is state machine '{Graph.Name}' hydrated?");

                verticesStack = BuildVerticesStack();
            }

            return verticesStack;
        }

        private List<Vertex> BuildVerticesStack()
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

            return result;
        }

        public IEnumerable<string> GetStateStack()
            => GetVerticesStack().Select(vertex => vertex.Name).ToArray();

        public async Task<bool> HydrateAsync(RootContext context)
        {
            Context = context;
            Context.Executor = this;

            await Inspector.AfterHydrateAsync(new StateMachineActionContext(Context));

            return true;
        }

        public async Task<RootContext> DehydrateAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' already dehydrated?");

            await Inspector.BeforeDehydrateAsync(new StateMachineActionContext(Context));

            var result = Context;
            result.Executor = null;
            Context = null;

            return result;
        }

        public bool Initialized => Context.StatesStack.Count > 0;

        public bool Finalized
        {
            get
            {
                var stack = GetVerticesStack();
                return stack.Count == 1 && stack.First().Type == VertexType.FinalState;
            }
        }

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

            Debug.WriteLine($"{Context.Id.Instance} initialized already");

            return false;
        }

        public async Task ExitAsync()
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                var currentStack = GetVerticesStack();

                foreach (var vertex in currentStack)
                {
                    await DoExitAsync(vertex);
                }

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

        public IEnumerable<Type> GetExpectedEvents()
        {
            var currentStack = GetVerticesStack();
            currentStack.Reverse();

            return currentStack.SelectMany(vertex => vertex.Edges).Select(edge => edge.TriggerType).Distinct();
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
            if (deferredEvents.Any() && deferredEvents.Contains(@event.EventName))
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
                if (!deferredEvents.Any() || !deferredEvents.Contains(@event.EventName))
                {
                    Context.DeferredEvents.Remove(@event);
                    Context.SetEvent(@event);
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

            var currentStack = GetVerticesStack(true);
            currentStack.Reverse();

            var deferred = true;

            if (!TryDeferEvent(@event))
            {
                deferred = false;

                foreach (var vertex in currentStack)
                {
                    foreach (var edge in vertex.Edges)
                    {
                        Context.SourceState = edge.SourceName;
                        Context.TargetState = edge.TargetName;

                        if (edge.Trigger == @event.EventName && await DoGuardAsync<TEvent>(edge))
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
            if (
                Graph.Initializers.TryGetValue(@event.EventName, out var initializer) ||
                (
                    @event.EventName == EventInfo<InitializationRequest>.Name &&
                    !Graph.Initializers.Any()
                )
            )
            {
                var context = new StateMachineInitializationContext(@event, Context);
                await Inspector.BeforeStateMachineInitializeAsync(context);

                if (initializer != null)
                {
                    await initializer.WhenAll(Context);
                }

                await Inspector.AfterStateMachineInitializeAsync(context);

                return true;
            }

            return false;
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
            var nextVertex = edge.Target;
            if (nextVertex != null)
            {
                Context.StatesStack.Reverse();
                foreach (var state in Context.StatesStack)
                {
                    if (Graph.AllVertices.TryGetValue(state, out var vertex))
                    {
                        if (vertex == nextVertex.Parent)
                        {
                            break;
                        }

                        await DoExitAsync(vertex);
                    }
                }
            }

            await DoEffectAsync<TEvent>(edge);

            if (nextVertex != null)
            {
                var previousNextVertex = nextVertex;

                Context.StatesStack.Clear();

                while (nextVertex != null)
                {
                    if (nextVertex.Parent != null && nextVertex.Parent.InitialVertex == nextVertex)
                    {
                        await DoInitializeStateAsync(nextVertex.Parent);
                    }

                    if (nextVertex.Type == VertexType.FinalState)
                    {
                        if (nextVertex.Parent is null)
                        {
                            await DoFinalizeStateMachineAsync();
                        }
                        else
                        {
                            await DoFinalizeStateAsync(nextVertex.Parent);
                        }

                        nextVertex = null;
                    }
                    else
                    {
                        await DoEntryAsync(nextVertex);

                        previousNextVertex = nextVertex;
                        nextVertex = nextVertex.InitialVertex;
                    }
                }

                nextVertex = previousNextVertex;
                while (nextVertex != null)
                {
                    Context.StatesStack.Add(nextVertex.Identifier);
                    nextVertex = nextVertex.Parent;
                }
                Context.StatesStack.Reverse();
            }
        }

        private async Task DoCompletion()
        {
            Context.SetEvent(new Completion());

            await DoProcessAsync(Context.Event);

            Context.ClearEvent();
        }

        public void Dispose()
        {
            Scope.Dispose();
        }

        private readonly IDictionary<Type, StateMachine> StateMachines = new Dictionary<Type, StateMachine>();

        public StateMachine GetStateMachine(Type stateMachineType, RootContext context)
        {
            lock (StateMachines)
            {
                if (!StateMachines.TryGetValue(stateMachineType, out var stateMachine))
                {
                    stateMachine = ServiceProvider.GetService(stateMachineType) as StateMachine;
                    stateMachine.Context = new StateMachineActionContext(context);

                    StateMachines.Add(stateMachineType, stateMachine);
                }

                return stateMachine;
            }
        }

        private readonly IDictionary<Type, BaseState> States = new Dictionary<Type, BaseState>();

        public TState GetState<TState>(IStateActionContext context)
            where TState : BaseState
        {
            if (!States.TryGetValue(typeof(TState), out var state))
            {
                state = ServiceProvider.GetService<TState>();

                States.Add(typeof(TState), state);
            }

            state.Context = context;

            return state as TState;
        }

        private readonly IDictionary<Type, object> Transitions = new Dictionary<Type, object>();

        public TTransition GetTransition<TTransition, TEvent>(ITransitionContext<TEvent> context)
            where TTransition : Transition<TEvent>
            where TEvent : Event, new()
        {
            if (!Transitions.TryGetValue(typeof(TTransition), out var transitionObj))
            {
                var transition = ServiceProvider.GetService<TTransition>();

                transition.Context = context;

                Transitions.Add(typeof(TTransition), transition);

                return transition;
            }
            else
            {
                (transitionObj as TTransition).Context = context;

                return transitionObj as TTransition;
            }
        }
    }
}
