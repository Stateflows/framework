using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
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
        IElseDefaultTransitionBuilder,
        IBehaviorBuilder,
        IForwardedEventBuilder<TEvent>,
        IInternal
    {
        public Edge Edge;

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

        public ITransitionBuilder<TEvent> AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
        {
            guardAsync.ThrowIfNull(nameof(guardAsync));

            guardAsync = guardAsync.AddStateMachineInvocationContext(Edge.Graph);

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
                        result = await guardAsync(context);
                    }
                    catch (Exception e)
                    {
                        if (e is StateflowsDefinitionException)
                        {
                            throw;
                        }
                        else
                        {
                            if (!await c.Executor.Inspector.OnTransitionGuardExceptionAsync(context, e))
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

            return this;
        }

        public ITransitionBuilder<TEvent> AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync)
        {
            effectAsync.ThrowIfNull(nameof(effectAsync));

            effectAsync = effectAsync.AddStateMachineInvocationContext(Edge.Graph);

            Edge.Effects.Actions.Add(async c =>
                {
                    if (transitiveVertexTypes.Contains(Edge.Source.Type))
                    {
                        c.SetEvent(new Completion().ToEventHolder());
                    }

                    var context = new TransitionContext<TEvent>(c, Edge);
                    try
                    {
                        await effectAsync(context);
                    }
                    catch (Exception e)
                    {
                        if (e is StateflowsDefinitionException)
                        {
                            throw;
                        }
                        else
                        {
                            if (!await c.Executor.Inspector.OnTransitionEffectExceptionAsync(context, e))
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

            return this;
        }

        IInternalTransitionBuilder<TEvent> IEffect<TEvent, IInternalTransitionBuilder<TEvent>>.AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync)
            => AddEffect(effectAsync) as IInternalTransitionBuilder<TEvent>;

        IInternalTransitionBuilder<TEvent> IBaseGuard<TEvent, IInternalTransitionBuilder<TEvent>>.AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddGuard(guardAsync) as IInternalTransitionBuilder<TEvent>;

        IElseTransitionBuilder<TEvent> IEffect<TEvent, IElseTransitionBuilder<TEvent>>.AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync)
            => AddEffect(effectAsync) as IElseTransitionBuilder<TEvent>;

        IElseInternalTransitionBuilder<TEvent> IEffect<TEvent, IElseInternalTransitionBuilder<TEvent>>.AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync)
            => AddEffect(effectAsync) as IElseInternalTransitionBuilder<TEvent>;

        IDefaultTransitionBuilder IDefaultEffect<IDefaultTransitionBuilder>.AddEffect(Func<ITransitionContext<Completion>, Task> effectAsync)
            => AddEffect(c => effectAsync(c as ITransitionContext<Completion>)) as IDefaultTransitionBuilder;

        IDefaultTransitionBuilder IBaseDefaultGuard<IDefaultTransitionBuilder>.AddGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync)
            => AddGuard(c => guardAsync(c as ITransitionContext<Completion>)) as IDefaultTransitionBuilder;

        IElseDefaultTransitionBuilder IDefaultEffect<IElseDefaultTransitionBuilder>.AddEffect(Func<ITransitionContext<Completion>, Task> effectAsync)
            => AddEffect(c => effectAsync(c as ITransitionContext<Completion>)) as IElseDefaultTransitionBuilder;

        IForwardedEventBuilder<TEvent> IBaseGuard<TEvent, IForwardedEventBuilder<TEvent>>.AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddGuard(guardAsync) as IForwardedEventBuilder<TEvent>;
    }
}
