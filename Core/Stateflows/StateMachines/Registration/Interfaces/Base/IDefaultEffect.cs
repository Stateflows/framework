using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDefaultEffect<out TReturn>
    {
        TReturn AddEffect(Func<ITransitionContext<Completion>, Task> effectAsync);

        TReturn AddEffect<TEffect>()
            where TEffect : class, IDefaultTransitionEffect
        {
            (this as IInternal).Services.AddServiceType<TEffect>();

            return AddEffect(c => (c as BaseContext).Context.Executor.GetDefaultTransitionEffect<TEffect>(c)?.EffectAsync());
        }

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
