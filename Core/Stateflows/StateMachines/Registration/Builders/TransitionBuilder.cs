using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;


namespace Stateflows.StateMachines.Registration.Builders
{
    internal class TransitionBuilder<TEvent> : ITransitionBuilder<TEvent>
        where TEvent : Event, new()
    {
        public Edge Edge;
       
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
                        await c.Executor.Inspector.OnTransitionGuardExceptionAsync(context, e);
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
                        await c.Executor.Inspector.OnTransitionEffectExceptionAsync(context, e);
                    }
                }
            );

            return this;
        }
    }
}
