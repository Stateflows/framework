using System;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class TransitionBuilder<TEvent> : ITransitionBuilder<TEvent>
        where TEvent : Event, new()
    {
        private Edge Edge;
        public TransitionBuilder(Edge edge)
        {
            Edge = edge;
        }

        public ITransitionBuilder<TEvent> AddGuard(GuardDelegateAsync<TEvent> guardAsync)
        {
            if (guardAsync != null)
            {
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
                            await c.Executor.Observer.OnTransitionGuardExceptionAsync(context, e);
                        }

                        return result;
                    }
                );
            }

            return this;
        }

        public ITransitionBuilder<TEvent> AddEffect(EffectDelegateAsync<TEvent> effectAsync)
        {
            if (effectAsync != null)
            {
                Edge.Effects.Actions.Add(async c =>
                    {
                        var context = new TransitionContext<TEvent>(c, Edge);
                        try
                        {
                            await effectAsync(context);
                        }
                        catch (Exception e)
                        {
                            await c.Executor.Observer.OnTransitionEffectExceptionAsync(context, e);
                        }
                    }
                );
            }

            return this;
        }
    }
}
