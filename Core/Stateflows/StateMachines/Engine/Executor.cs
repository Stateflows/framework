﻿using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Exceptions;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context;
using Stateflows.StateMachines.Extensions;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines.Engine
{
    internal sealed class Executor : IDisposable, IStateflowsExecutor
    {
        public readonly Graph Graph;

        public bool StateHasChanged;

        public StateMachinesRegister Register { get; set; }

        public IServiceProvider ServiceProvider => ScopesStack.Peek().ServiceProvider;

        private readonly Stack<IServiceScope> ScopesStack = new Stack<IServiceScope>();

        private EventStatus EventStatus;
        private bool IsEventStatusOverriden;

        public void OverrideEventStatus(EventStatus eventStatus)
        {
            EventStatus = eventStatus;
            IsEventStatusOverriden = true;
        }

        public Executor(StateMachinesRegister register, Graph graph, IServiceProvider serviceProvider, StateflowsContext stateflowsContext, EventHolder @event)
        {
            Register = register;
            ScopesStack.Push(serviceProvider.CreateScope());
            Graph = graph;
            Context = new RootContext(stateflowsContext, this, @event);
            var logger = ServiceProvider.GetService<ILogger<Executor>>();
            Inspector = new Inspector(this, logger);
        }

        public void Dispose()
        {
            while (ScopesStack.Any())
            {
                EndScope();
            }
        }

        public void BeginScope()
        {
            ScopesStack.Push(ServiceProvider.CreateScope());
        }

        public void EndScope()
        {
            var scope = ScopesStack.Pop();
            scope.Dispose();
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
                        StateHasChanged = true;
                    }

                    break;
                }
            }

            VerticesStack = result;
        }

        public IEnumerable<string> GetStateStack()
            => VerticesStack.Select(vertex => vertex.Name).ToArray();

        public Task HydrateAsync()
        {
            RebuildVerticesStack();

            return Inspector.AfterHydrateAsync(new StateMachineActionContext(Context));
        }

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

        public async Task<EventStatus> InitializeAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (!Initialized)
            {
                var result = await DoInitializeStateMachineAsync(eventHolder);

                if (
                    result == InitializationStatus.InitializedImplicitly ||
                    result == InitializationStatus.InitializedExplicitly
                )
                {
                    await DoInitializeCascadeAsync(Graph.InitialVertex);

                    await DoCompletionAsync();

                    if (result == InitializationStatus.InitializedExplicitly)
                    {
                        return EventStatus.Initialized;
                    }
                    else
                    {
                        if (eventHolder.Payload is Initialize)
                        {
                            return EventStatus.Initialized;
                        }
                        else
                        {
                            return EventStatus.Consumed;
                        }
                    }
                }
                else
                {
                    return result == InitializationStatus.NoSuitableInitializer
                        ? EventStatus.Rejected
                        : EventStatus.NotInitialized;
                }
            }

            return EventStatus.NotInitialized;
        }

        public async Task<bool> ExitAsync()
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
                StateHasChanged = true;

                return true;
            }

            return false;
        }

        public void Reset(ResetMode resetMode)
        {
            Debug.Assert(Context != null, $"Context is unavailable. Is state machine '{Graph.Name}' hydrated?");

            if (Initialized)
            {
                Context.Context.Values.Clear();

                if (resetMode != ResetMode.KeepVersionAndSubscriptions) // KeepSubscriptions || Full
                {
                    Context.Context.Version = 0;

                    if (resetMode != ResetMode.KeepSubscriptions) // Full
                    {
                        Context.Context.Deleted = true;
                    }
                }
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
                StateHasChanged = true;

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
                StateHasChanged = true;

                vertex = vertex.InitialVertex;
            }
        }

        public IEnumerable<string> GetExpectedEventNames()
            => GetExpectedEvents()
                .Where(type => !type.IsSubclassOf(typeof(TimeEvent)))
                .Where(type => type != typeof(Startup))
                .Where(type => type != typeof(Completion))
                .Select(type => type.GetEventName())
                .ToArray();

        public IEnumerable<Type> GetExpectedEvents()
        {
            var currentStack = VerticesStack.ToList();
            currentStack.Reverse();

            return currentStack.Any()
                ? currentStack
                    .SelectMany(vertex => vertex.Edges.Values)
                    .SelectMany(edge => edge.ActualTriggerTypes)
                    .Distinct()
                    .ToArray()
                : Graph.InitializerTypes
                    .ToArray();
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

        public async Task<EventStatus> ProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            StateHasChanged = false;

            var result = EventStatus.Rejected;

            if (Initialized)
            {
                result = await DoProcessAsync(eventHolder);
            }

            if (IsEventStatusOverriden)
            {
                result = EventStatus;
            }

            return result;
        }

        private bool TryDeferEvent<TEvent>(EventHolder<TEvent> eventHolder)

        {
            var deferredEvents = GetDeferredEvents();
            if (deferredEvents.Any() && deferredEvents.Contains(eventHolder.Name))
            {
                Context.DeferredEvents.Add(eventHolder);
                return true;
            }
            return false;
        }

        private async Task DispatchNextDeferredEvent()
        {
            var deferredEvents = GetDeferredEvents();
            foreach (var eventHolder in Context.DeferredEvents)
            {
                if (!deferredEvents.Any() || !deferredEvents.Contains(eventHolder.Name))
                {
                    Context.DeferredEvents.Remove(eventHolder);

                    Context.SetEvent(eventHolder);

                    RebuildVerticesStack();

                    await eventHolder.DoProcessAsync(this);

                    Context.ClearEvent();

                    break;
                }
            }
        }

        Task<EventStatus> IStateflowsExecutor.DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
            => DoProcessAsync(eventHolder);

        private async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
        {
            Debug.Assert(Context != null, $"Context is not available. Is state machine '{Graph.Name}' hydrated?");

            var currentStack = VerticesStack.ToList();
            currentStack.Reverse();

            var deferred = true;

            if (!TryDeferEvent(eventHolder))
            {
                deferred = false;

                foreach (var vertex in currentStack)
                {
                    var edges = vertex.OrderedEdges
                        .SelectMany(edge => edge.GetActualEdges())
                        .ToArray();

                    foreach (var edge in edges)
                    {
                        if (eventHolder.Triggers(edge) && await DoGuardAsync<TEvent>(edge))
                        {
                            Context.AddExecutionStep(
                                edge.SourceName,
                                edge.TargetName == string.Empty
                                    ? null
                                    : edge.TargetName,
                                eventHolder.Payload
                            );

                            await DoConsumeAsync<TEvent>(edge);

                            await DoCompletionAsync();

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
        {
            BeginScope();

            var context = new GuardContext<TEvent>(Context, edge);

            await Inspector.BeforeTransitionGuardAsync(context);

            var result = await edge.Guards.WhenAll(Context);

            await Inspector.AfterGuardAsync(context, result);


            return result;
        }

        private async Task DoEffectAsync<TEvent>(Edge edge)

        {
            BeginScope();

            var context = new TransitionContext<TEvent>(Context, edge);

            await Inspector.BeforeEffectAsync(context);

            await edge.Effects.WhenAll(Context);

            await Inspector.AfterEffectAsync(context);

            EndScope();
        }

        public async Task<InitializationStatus> DoInitializeStateMachineAsync(EventHolder @event)
        {
            BeginScope();

            InitializationStatus result;

            var initializer = Graph.Initializers.ContainsKey(@event.Name)
                ? Graph.Initializers[@event.Name]
                : Graph.DefaultInitializer;

            var context = new StateMachineInitializationContext(Context);
            await Inspector.BeforeStateMachineInitializeAsync(context);

            if (initializer != null)
            {
                try
                {
                    result = await initializer.WhenAll(Context)
                        ? initializer == Graph.DefaultInitializer
                            ? InitializationStatus.InitializedImplicitly
                            : InitializationStatus.InitializedExplicitly
                        : InitializationStatus.NotInitialized;
                }
                catch (Exception e)
                {
                    if (!await Inspector.OnStateMachineInitializationExceptionAsync(context, e))
                    {
                        throw;
                    }
                    else
                    {
                        throw new BehaviorExecutionException(e);
                    }
                }
            }
            else
            {
                result = Graph.Initializers.Any()
                    ? InitializationStatus.NoSuitableInitializer
                    : InitializationStatus.InitializedImplicitly;
            }

            await Inspector.AfterStateMachineInitializeAsync(
                context,
                result == InitializationStatus.InitializedImplicitly ||
                result == InitializationStatus.InitializedExplicitly
            );

            EndScope();

            return result;
        }

        public async Task DoFinalizeStateMachineAsync()
        {
            BeginScope();

            var context = new StateMachineActionContext(Context);
            await Inspector.BeforeStateMachineFinalizeAsync(context);

            try
            {
                await Graph.Finalize.WhenAll(Context);
            }
            catch (Exception e)
            {
                if (!await Inspector.OnStateMachineFinalizationExceptionAsync(context, e))
                {
                    throw;
                }
                else
                {
                    throw new BehaviorExecutionException(e);
                }
            }

            await Inspector.AfterStateMachineFinalizeAsync(context);

            EndScope();
        }

        public async Task DoInitializeStateAsync(Vertex vertex)
        {
            BeginScope();

            var context = new StateActionContext(Context, vertex, Constants.Initialize);
            await Inspector.BeforeStateInitializeAsync(context);

            await vertex.Initialize.WhenAll(Context);

            await Inspector.AfterStateInitializeAsync(context);

            EndScope();
        }

        public async Task DoFinalizeStateAsync(Vertex vertex)
        {
            BeginScope();

            var context = new StateActionContext(Context, vertex, Constants.Finalize);
            await Inspector.BeforeStateFinalizeAsync(context);

            await vertex.Finalize.WhenAll(Context);

            await Inspector.AfterStateFinalizeAsync(context);

            EndScope();
        }

        public async Task DoEntryAsync(Vertex vertex)
        {
            BeginScope();

            var context = new StateActionContext(Context, vertex, Constants.Entry);
            await Inspector.BeforeStateEntryAsync(context);

            await vertex.Entry.WhenAll(Context);

            await Inspector.AfterStateEntryAsync(context);

            EndScope();
        }

        private async Task DoExitAsync(Vertex vertex)
        {
            BeginScope();

            var context = new StateActionContext(Context, vertex, Constants.Exit);
            await Inspector.BeforeStateExitAsync(context);

            await vertex.Exit.WhenAll(Context);

            await Inspector.AfterStateExitAsync(context);

            EndScope();
        }

        private async Task DoConsumeAsync<TEvent>(Edge edge)

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
                    StateHasChanged = true;
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
                    StateHasChanged = true;
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

        private async Task<bool> DoCompletionAsync()
        {
            var completionEventHolder = (new Completion()).ToEventHolder();
            Context.SetEvent(completionEventHolder);

            RebuildVerticesStack();

            var result = await DoProcessAsync(completionEventHolder);

            Context.ClearEvent();

            return result == EventStatus.Consumed;
        }

        public IStateMachine GetStateMachine(Type stateMachineType)
        {
            var stateMachine = ServiceProvider.GetService(stateMachineType) as IStateMachine;

            return stateMachine;
        }

        public TDefaultInitializer GetDefaultInitializer<TDefaultInitializer>(IStateMachineInitializationContext context)
            where TDefaultInitializer : class, IDefaultInitializer
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var initializer = ServiceProvider.GetService<TDefaultInitializer>();

            return initializer;
        }

        public TInitializer GetInitializer<TInitializer, TInitializationEvent>(IStateMachineInitializationContext<TInitializationEvent> context)
            where TInitializer : class, IInitializer<TInitializationEvent>
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var initializer = ServiceProvider.GetService<TInitializer>();

            return initializer;
        }

        public TFinalizer GetFinalizer<TFinalizer>(IStateMachineActionContext context)
            where TFinalizer : class, IFinalizer
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var initializer = ServiceProvider.GetService<TFinalizer>();

            return initializer;
        }

        public TState GetState<TState>(IStateActionContext context)
            where TState : class, IState
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = context.CurrentState.Values;
            ContextValues.SourceStateValuesHolder.Value = null;
            ContextValues.TargetStateValuesHolder.Value = null;

            StateMachinesContextHolder.StateContext.Value = context.CurrentState;
            StateMachinesContextHolder.TransitionContext.Value = null;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var state = ServiceProvider.GetService<TState>();

            return state;
        }

        public TTransition GetTransition<TTransition, TEvent>(ITransitionContext<TEvent> context)
            where TTransition : class, ITransition<TEvent>

        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TTransition>();

            return transition;
        }

        public TTransitionGuard GetTransitionGuard<TTransitionGuard, TEvent>(ITransitionContext<TEvent> context)
            where TTransitionGuard : class, ITransitionGuard<TEvent>

        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TTransitionGuard>();

            return transition;
        }

        public TTransitionEffect GetTransitionEffect<TTransitionEffect, TEvent>(ITransitionContext<TEvent> context)
            where TTransitionEffect : class, ITransitionEffect<TEvent>

        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TTransitionEffect>();

            return transition;
        }

        public TDefaultTransition GetDefaultTransition<TDefaultTransition>(ITransitionContext<Completion> context)
            where TDefaultTransition : class, IDefaultTransition
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TDefaultTransition>();

            return transition;
        }

        public TDefaultTransitionGuard GetDefaultTransitionGuard<TDefaultTransitionGuard>(ITransitionContext<Completion> context)
            where TDefaultTransitionGuard : class, IDefaultTransitionGuard
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TDefaultTransitionGuard>();

            return transition;
        }

        public TDefaultTransitionEffect GetDefaultTransitionEffect<TDefaultTransitionEffect>(ITransitionContext<Completion> context)
            where TDefaultTransitionEffect : class, IDefaultTransitionEffect
        {
            ContextValues.GlobalValuesHolder.Value = context.StateMachine.Values;
            ContextValues.StateValuesHolder.Value = null;
            ContextValues.SourceStateValuesHolder.Value = context.SourceState.Values;
            ContextValues.TargetStateValuesHolder.Value = context.TargetState?.Values;

            StateMachinesContextHolder.StateContext.Value = null;
            StateMachinesContextHolder.TransitionContext.Value = context;
            StateMachinesContextHolder.StateMachineContext.Value = context.StateMachine;
            StateMachinesContextHolder.ExecutionContext.Value = context;

            var transition = ServiceProvider.GetService<TDefaultTransitionEffect>();

            return transition;
        }
    }
}
