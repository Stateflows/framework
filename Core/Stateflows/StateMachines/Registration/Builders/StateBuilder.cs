using System;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal partial class StateBuilder : IStateBuilder, IStateBuilderInternal, IStateTransitionsBuilder
    {
        public Vertex Vertex { get; }

        public IServiceCollection Services { get; }

        public StateBuilder(Vertex vertex, IServiceCollection services)
        {
            Vertex = vertex;
            Services = services;
        }

        #region Events
        public IStateBuilder AddOnInitialize(StateActionDelegateAsync stateActionAsync)
        {
            if (stateActionAsync == null)
                throw new ArgumentNullException("Action not provided");

            Vertex.Initialize.Actions.Add(async c =>
            {
                var context = new StateActionContext(c, Vertex, Constants.Entry);
                try
                {
                    await stateActionAsync(context);
                }
                catch (Exception e)
                {
                    await c.Executor.Observer.OnStateInitializeExceptionAsync(context, e);
                }
            }
            );

            return this;
        }

        public IStateBuilder AddOnEntry(StateActionDelegateAsync stateActionAsync)
        {
            if (stateActionAsync == null)
                throw new ArgumentNullException("Action not provided");

            Vertex.Entry.Actions.Add(async c =>
                {
                    var context = new StateActionContext(c, Vertex, Constants.Entry);
                    try
                    {
                        await stateActionAsync(context);
                    }
                    catch (Exception e)
                    {
                        await c.Executor.Observer.OnStateEntryExceptionAsync(context, e);
                    }
                }
            );

            return this;
        }

        public IStateBuilder AddOnExit(StateActionDelegateAsync stateActionAsync)
        {
            if (stateActionAsync == null)
                throw new ArgumentNullException("Action not provided");

            Vertex.Exit.Actions.Add(async c =>
                {
                    var context = new StateActionContext(c, Vertex, Constants.Exit);
                    try
                    {
                        await stateActionAsync(context);
                    }
                    catch (Exception e)
                    {
                        await c.Executor.Observer.OnStateExitExceptionAsync(context, e);
                    }
                }
            );

            return this;
        }
        #endregion

        #region Transitions
        public IStateBuilder AddTransition<TEvent>(string targetStateName, TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
        {
            var edge = new Edge()
            {
                Trigger = EventInfo<TEvent>.Name,
                TriggerType = typeof(TEvent),
                Graph = Vertex.Graph,
                SourceName = Vertex.Name,
                TargetName = targetStateName,
            };

            Vertex.Edges.Add(edge);
            Vertex.Graph.AllEdges.Add(edge);

            if (transitionBuildAction != null)
            {
                transitionBuildAction(new TransitionBuilder<TEvent>(edge));
            }

            return this;
        }

        public IStateBuilder AddDefaultTransition(string targetStateName, TransitionBuilderAction<Completion> transitionBuildAction = null)
            => AddTransition<Completion>(targetStateName, transitionBuildAction);

        public IStateBuilder AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction = null)
            where TEvent : Event, new()
            => AddTransition<TEvent>(Constants.DefaultTransitionTarget, transitionBuildAction);

        IStateTransitionsBuilder IStateTransitionsBuilderBase<IStateTransitionsBuilder>.AddTransition<TEvent>(string targetStateName, TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddTransition<TEvent>(targetStateName, transitionBuildAction) as IStateTransitionsBuilder;

        IStateTransitionsBuilder IStateTransitionsBuilderBase<IStateTransitionsBuilder>.AddDefaultTransition(string targetStateName, TransitionBuilderAction<Completion> transitionBuildAction)
            => AddDefaultTransition(targetStateName, transitionBuildAction) as IStateTransitionsBuilder;

        IStateTransitionsBuilder IStateTransitionsBuilderBase<IStateTransitionsBuilder>.AddInternalTransition<TEvent>(TransitionBuilderAction<TEvent> transitionBuildAction)
            => AddInternalTransition<TEvent>(transitionBuildAction) as IStateTransitionsBuilder;
        #endregion
    }
}

