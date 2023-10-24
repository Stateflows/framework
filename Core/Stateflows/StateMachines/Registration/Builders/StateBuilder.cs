using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Events;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using System.Security.Cryptography.X509Certificates;
using Stateflows.Activities;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal partial class StateBuilder : 
        IStateBuilder,
        ISubmachineStateBuilder, 
        ITypedStateBuilder,
        ISubmachineTypedStateBuilder,
        IInternal
    {
        public Vertex Vertex { get; }

        public IServiceCollection Services { get; }

        public StateBuilder(Vertex vertex, IServiceCollection services)
        {
            Vertex = vertex;
            Services = services;
        }

        #region Events
        public IStateBuilder AddOnInitialize(Func<IStateActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

            Vertex.Initialize.Actions.Add(async c =>
            {
                var context = new StateActionContext(c, Vertex, Constants.Entry);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Inspector.OnStateInitializeExceptionAsync(context, e);
                }
            }
            );

            return this;
        }

        public IStateBuilder AddOnFinalize(Func<IStateActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

            Vertex.Finalize.Actions.Add(async c =>
            {
                var context = new StateActionContext(c, Vertex, Constants.Entry);
                try
                {
                    await actionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Inspector.OnStateFinalizeExceptionAsync(context, e);
                }
            }
            );

            return this;
        }

        public IStateBuilder AddOnEntry(Func<IStateActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

            Vertex.Entry.Actions.Add(async c =>
                {
                    var context = new StateActionContext(c, Vertex, Constants.Entry);
                    try
                    {
                        await actionAsync(context);
                    }
                    catch (Exception e)
                    {
                        await c.Executor.Inspector.OnStateEntryExceptionAsync(context, e);
                    }
                }
            );

            return this;
        }

        public IStateBuilder AddOnExit(Func<IStateActionContext, Task> actionAsync)
        {
            actionAsync.ThrowIfNull(nameof(actionAsync));

            actionAsync = actionAsync.AddStateMachineInvocationContext(Vertex.Graph);

            Vertex.Exit.Actions.Add(async c =>
                {
                    var context = new StateActionContext(c, Vertex, Constants.Exit);
                    try
                    {
                        await actionAsync(context);
                    }
                    catch (Exception e)
                    {
                        await c.Executor.Inspector.OnStateExitExceptionAsync(context, e);
                    }
                }
            );

            return this;
        }
        #endregion

        #region Utils
        public IStateBuilder AddDeferredEvent<TEvent>() where TEvent : Event
        {
            if (typeof(TEvent) == typeof(Completion))
                throw new Exception("Completion event cannot be deferred.");

            if (typeof(TEvent) == typeof(Exit))
                throw new Exception("Exit event cannot be deferred.");

            Vertex.DeferredEvents.Add(EventInfo<TEvent>.Name);

            return this;
        }
        #endregion

        #region Transitions
        public IStateBuilder AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event
        {
            var edge = new Edge()
            {
                Trigger = EventInfo<TEvent>.Name,
                TriggerType = typeof(TEvent),
                Graph = Vertex.Graph,
                SourceName = Vertex.Name,
                Source = Vertex,
                TargetName = targetVertexName,
            };

            Vertex.Edges.Add(edge);
            Vertex.Graph.AllEdges.Add(edge);

            transitionBuildAction?.Invoke(new TransitionBuilder<TEvent>(edge));

            return this;
        }

        public IStateBuilder AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction = null)
            => AddTransition<Completion>(targetVertexName, transitionBuildAction);

        public IStateBuilder AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, transitionBuildAction);

        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ITypedStateBuilder;

        ITypedStateBuilder IStateTransitions<ITypedStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ITypedStateBuilder;

        ITypedStateBuilder IStateUtils<ITypedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ITypedStateBuilder;
        #endregion

        #region Submachine
        public ISubmachineStateBuilder AddSubmachine(string submachineName, StateActionInitializationBuilder initializationBuilder = null)
        {
            Vertex.SubmachineName = submachineName;
            Vertex.SubmachineInitializationBuilder = initializationBuilder;

            return this;
        }

        ISubmachineTypedStateBuilder IStateSubmachine<ISubmachineTypedStateBuilder>.AddSubmachine(string submachineName, StateActionInitializationBuilder initializationBuilder)
            => AddSubmachine(submachineName, initializationBuilder) as ISubmachineTypedStateBuilder;

        ISubmachineStateBuilder IStateEntry<ISubmachineStateBuilder>.AddOnEntry(Func<IStateActionContext, Task> actionAsync)
            => AddOnEntry(actionAsync) as ISubmachineStateBuilder;

        ISubmachineStateBuilder IStateExit<ISubmachineStateBuilder>.AddOnExit(Func<IStateActionContext, Task> actionAsync)
            => AddOnExit(actionAsync) as ISubmachineStateBuilder;

        ISubmachineStateBuilder IStateUtils<ISubmachineStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ISubmachineStateBuilder;

        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ISubmachineStateBuilder;

        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ISubmachineStateBuilder;

        ISubmachineStateBuilder IStateTransitions<ISubmachineStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ISubmachineStateBuilder;

        ISubmachineTypedStateBuilder IStateUtils<ISubmachineTypedStateBuilder>.AddDeferredEvent<TEvent>()
            => AddDeferredEvent<TEvent>() as ISubmachineTypedStateBuilder;

        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddTransition<TEvent>(string targetVertexName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetVertexName, transitionBuildAction) as ISubmachineTypedStateBuilder;

        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddDefaultTransition(string targetVertexName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetVertexName, transitionBuildAction) as ISubmachineTypedStateBuilder;

        ISubmachineTypedStateBuilder IStateTransitions<ISubmachineTypedStateBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as ISubmachineTypedStateBuilder;
        #endregion
    }
}

