using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IEffect<out TEvent, out TReturn>
    {
        TReturn AddEffect(Func<ITransitionContext<TEvent>, Task> effectAsync);
        
        [DebuggerHidden]
        public TReturn AddEffect(Action<ITransitionContext<TEvent>> effect)
            => AddEffect(effect
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );

        TReturn AddEffect<TEffect>()
            where TEffect : class, ITransitionEffect<TEvent>
            => AddEffect(c => ((BaseContext)c).Context.Executor.GetTransitionEffect<TEffect, TEvent>(c)?.EffectAsync(c.Event));

        TReturn AddEffects<TEffect1, TEffect2>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
        {
            AddEffect<TEffect1>();
            return AddEffect<TEffect2>();
        }

        TReturn AddEffects<TEffect1, TEffect2, TEffect3>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2>();
            return AddEffect<TEffect3>();
        }

        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
            where TEffect4 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2, TEffect3>();
            return AddEffect<TEffect4>();
        }

        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4, TEffect5>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
            where TEffect4 : class, ITransitionEffect<TEvent>
            where TEffect5 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>();
            return AddEffect<TEffect5>();
        }
    }
}
