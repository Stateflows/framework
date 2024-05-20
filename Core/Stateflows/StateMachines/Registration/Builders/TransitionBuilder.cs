using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Events;
using Stateflows.Common.Exceptions;

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
        IForwardedEventBuilder<TEvent>
        where TEvent : Event, new()
    {
        public Edge Edge;

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(nameof(StateMachine), Edge.Source.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Edge.Source.Graph.Version;

        public TransitionBuilder(Edge edge)
        {
            Edge = edge;
        }

        public ITransitionBuilder<TEvent> AddGuard(Func<IGuardContext<TEvent>, Task<bool>> guardAsync)
        {
            guardAsync.ThrowIfNull(nameof(guardAsync));

            guardAsync = guardAsync.AddStateMachineInvocationContext(Edge.Graph);

            Edge.Guards.Actions.Add(async c =>
                {
                    var context = new GuardContext<TEvent>(c, Edge);
                    var result = false;
                    try
                    {
                        result = await guardAsync(context);
                    }
                    catch (Exception e)
                    {
                        if (e is StateflowsException)
                        {
                            throw;
                        }
                        else
                        {
                            await c.Executor.Inspector.OnTransitionGuardExceptionAsync(context, e);
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
                    var context = new TransitionContext<TEvent>(c, Edge);
                    try
                    {
                        await effectAsync(context);
                    }
                    catch (Exception e)
                    {
                        if (e is StateflowsException)
                        {
                            throw;
                        }
                        else
                        {
                            await c.Executor.Inspector.OnTransitionEffectExceptionAsync(context, e);
                        }
                    }
                }
            );

            return this;
        }

        IInternalTransitionBuilder<TEvent> IEffect<TEvent, IInternalTransitionBuilder<TEvent>>.AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync)
            => AddEffect(effectAsync) as IInternalTransitionBuilder<TEvent>;

        IInternalTransitionBuilder<TEvent> IGuard<TEvent, IInternalTransitionBuilder<TEvent>>.AddGuard(Func<IGuardContext<TEvent>, Task<bool>> guardAsync)
            => AddGuard(guardAsync) as IInternalTransitionBuilder<TEvent>;

        IElseTransitionBuilder<TEvent> IEffect<TEvent, IElseTransitionBuilder<TEvent>>.AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync)
            => AddEffect(effectAsync) as IElseTransitionBuilder<TEvent>;

        IElseInternalTransitionBuilder<TEvent> IEffect<TEvent, IElseInternalTransitionBuilder<TEvent>>.AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync)
            => AddEffect(effectAsync) as IElseInternalTransitionBuilder<TEvent>;

        IDefaultTransitionBuilder IEffect<CompletionEvent, IDefaultTransitionBuilder>.AddEffect(Func<ITransitionContext<CompletionEvent>, Task> effectAsync)
            => AddEffect(c => effectAsync(c as ITransitionContext<CompletionEvent>)) as IDefaultTransitionBuilder;

        IDefaultTransitionBuilder IGuard<CompletionEvent, IDefaultTransitionBuilder>.AddGuard(Func<IGuardContext<CompletionEvent>, Task<bool>> guardAsync)
            => AddGuard(c => guardAsync(c as IGuardContext<CompletionEvent>)) as IDefaultTransitionBuilder;

        IElseDefaultTransitionBuilder IEffect<CompletionEvent, IElseDefaultTransitionBuilder>.AddEffect(Func<ITransitionContext<CompletionEvent>, Task> effectAsync)
            => AddEffect(c => effectAsync(c as ITransitionContext<CompletionEvent>)) as IElseDefaultTransitionBuilder;

        IForwardedEventBuilder<TEvent> IGuard<TEvent, IForwardedEventBuilder<TEvent>>.AddGuard(Func<IGuardContext<TEvent>, Task<bool>> guardAsync)
            => AddGuard(guardAsync) as IForwardedEventBuilder<TEvent>;
    }
}
