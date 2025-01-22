using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDefaultEffect<out TReturn>
    {
        TReturn AddEffect(Func<ITransitionContext<Completion>, Task> effectAsync);
        
        [DebuggerHidden]
        public TReturn AddEffect(Action<ITransitionContext<Completion>> effect)
            => AddEffect(effect
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );

        TReturn AddEffect<TEffect>()
            where TEffect : class, IDefaultTransitionEffect
            => AddEffect(c => ((BaseContext)c).Context.Executor.GetDefaultTransitionEffect<TEffect>(c)?.EffectAsync());

        TReturn AddEffects<TEffect1, TEffect2>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
        {
            AddEffect<TEffect1>();
            return AddEffect<TEffect2>();
        }

        TReturn AddEffects<TEffect1, TEffect2, TEffect3>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
            where TEffect3 : class, IDefaultTransitionEffect
        {
            AddEffects<TEffect1, TEffect2>();
            return AddEffect<TEffect3>();
        }

        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
            where TEffect3 : class, IDefaultTransitionEffect
            where TEffect4 : class, IDefaultTransitionEffect
        {
            AddEffects<TEffect1, TEffect2, TEffect3>();
            return AddEffect<TEffect4>();
        }

        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4, TEffect5>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
            where TEffect3 : class, IDefaultTransitionEffect
            where TEffect4 : class, IDefaultTransitionEffect
            where TEffect5 : class, IDefaultTransitionEffect
        {
            AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>();
            return AddEffect<TEffect5>();
        }
    }
}
