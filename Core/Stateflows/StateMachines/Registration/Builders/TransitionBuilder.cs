using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class TransitionBuilder<TEvent> :
        ITransitionBuilder<TEvent>,
        IElseTransitionBuilder<TEvent>,
        IInternalTransitionBuilder<TEvent>,
        IElseInternalTransitionBuilder<TEvent>,
        IDefaultTransitionBuilder,
        IDefaultTransitionEffectBuilder,
        IElseDefaultTransitionBuilder,
        IBehaviorBuilder,
        IForwardedEventBuilder<TEvent>,
        IInternal,
        IEdgeBuilder
    {
        public Edge Edge { get; private set; }

        private readonly IEnumerable<VertexType> transitiveVertexTypes = new HashSet<VertexType>() {
            VertexType.Junction,
            VertexType.Choice,
        };

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Edge.Source.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Edge.Source.Graph.Version;

        public IServiceCollection Services { get; }

        public TransitionBuilder(Edge edge, IServiceCollection services)
        {
            Edge = edge;
            Services = services;
        }

        public ITransitionBuilder<TEvent> AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
        {
            foreach (var guardAsync in guardsAsync)
            {
                guardAsync.ThrowIfNull(nameof(guardAsync));

                if (Edge.Source.Type == VertexType.Fork)
                {
                    throw new TransitionDefinitionException(
                        $"Transition outgoing from fork '{Edge.SourceName}' cannot have guards",
                        Edge.Graph.Class
                    );
                }

                var guardHandler = guardAsync.AddStateMachineInvocationContext(Edge.Graph);

                Edge.Guards.Actions.Add(async c =>
                    {
                        if (transitiveVertexTypes.Contains(Edge.Source.Type))
                        {
                            c.SetEvent(new Completion().ToEventHolder());
                        }

                        var context = new GuardContext<TEvent>(c, Edge);
                        var result = false;
                        try
                        {
                            result = await guardHandler(context);
                        }
                        catch (Exception e)
                        {
                            if (e is StateflowsDefinitionException)
                            {
                                throw;
                            }
                            else
                            {
                                var inspector = await c.Executor.GetInspectorAsync();

                                if (!await inspector.OnTransitionGuardExceptionAsync(context, e))
                                {
                                    throw;
                                }
                                else
                                {
                                    throw new BehaviorExecutionException(e);
                                }
                            }
                        }
                        finally
                        {
                            if (transitiveVertexTypes.Contains(Edge.Source.Type))
                            {
                                c.ClearEvent();
                            }
                        }

                        return result;
                    }
                );
            }

            return this;
        }

        public ITransitionBuilder<TEvent> AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
        {
            foreach (var effectAsync in effectsAsync)
            {
                effectAsync.ThrowIfNull(nameof(effectAsync));

                var effectHandler = effectAsync.AddStateMachineInvocationContext(Edge.Graph);

                Edge.Effects.Actions.Add(async c =>
                    {
                        if (transitiveVertexTypes.Contains(Edge.Source.Type))
                        {
                            c.SetEvent(new Completion().ToEventHolder());
                        }

                        var context = new TransitionContext<TEvent>(c, Edge);
                        try
                        {
                            await effectHandler(context);
                        }
                        catch (Exception e)
                        {
                            if (e is StateflowsDefinitionException)
                            {
                                throw;
                            }
                            else
                            {
                                var inspector = await c.Executor.GetInspectorAsync();

                                if (!await inspector.OnTransitionEffectExceptionAsync(context, e))
                                {
                                    throw;
                                }
                                else
                                {
                                    throw new BehaviorExecutionException(e);
                                }
                            }
                        }
                        finally
                        {
                            if (transitiveVertexTypes.Contains(Edge.Source.Type))
                            {
                                c.ClearEvent();
                            }
                        }
                    }
                );
            }

            return this;
        }

        IInternalTransitionBuilder<TEvent> IEffect<TEvent, IInternalTransitionBuilder<TEvent>>.AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IInternalTransitionBuilder<TEvent>;

        IInternalTransitionBuilder<TEvent> IBaseGuard<TEvent, IInternalTransitionBuilder<TEvent>>.AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync) as IInternalTransitionBuilder<TEvent>;

        IElseTransitionBuilder<TEvent> IEffect<TEvent, IElseTransitionBuilder<TEvent>>.AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IElseTransitionBuilder<TEvent>;

        IElseInternalTransitionBuilder<TEvent> IEffect<TEvent, IElseInternalTransitionBuilder<TEvent>>.AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync)
            => AddEffect(effectsAsync) as IElseInternalTransitionBuilder<TEvent>;

        IDefaultTransitionBuilder IDefaultEffect<IDefaultTransitionBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IDefaultTransitionBuilder;

        IDefaultTransitionEffectBuilder IDefaultEffect<IDefaultTransitionEffectBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IDefaultTransitionEffectBuilder;

        IElseDefaultTransitionBuilder IDefaultEffect<IElseDefaultTransitionBuilder>.AddEffect(params Func<ITransitionContext<Completion>, Task>[] effectsAsync)
            => (this as TransitionBuilder<Completion>)!.AddEffect(effectsAsync) as IElseDefaultTransitionBuilder;

        IDefaultTransitionBuilder IBaseDefaultGuard<IDefaultTransitionBuilder>.AddGuard(params Func<ITransitionContext<Completion>, Task<bool>>[] guardsAsync)
            => (this as TransitionBuilder<Completion>)!.AddGuard(guardsAsync) as IDefaultTransitionBuilder;

        IForwardedEventBuilder<TEvent> IBaseGuard<TEvent, IForwardedEventBuilder<TEvent>>.AddGuard(params Func<ITransitionContext<TEvent>, Task<bool>>[] guardsAsync)
            => AddGuard(guardsAsync) as IForwardedEventBuilder<TEvent>;
    }
}
