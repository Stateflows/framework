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
        /// <summary>
        /// Adds an asynchronous effect to the current transition.<br/>
        /// Use the following pattern to implement the effect:
        /// <code>async c => {
        ///     // effect logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="effectAsync">Asynchronous effect handler</param>
        TReturn AddEffect(Func<ITransitionContext<Completion>, Task> effectAsync);

        /// <summary>
        /// Adds a synchronous effect to the current transition.<br/>
        /// Use the following pattern to implement the effect:
        /// <code>c => {
        ///     // effect logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="effect">Synchronous effect handler</param>
        [DebuggerHidden]
        public TReturn AddEffect(Action<ITransitionContext<Completion>> effect)
            => AddEffect(effect
                .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                .ToAsync()
            );

        /// <summary>
        /// Adds a typed effect to the current transition.
        /// </summary>
        /// <typeparam name="TEffect">The type of the effect handler.</typeparam>
        TReturn AddEffect<TEffect>()
            where TEffect : class, IDefaultTransitionEffect
            => AddEffect(c => ((BaseContext)c).Context.Executor.GetDefaultTransitionEffect<TEffect>(c)?.EffectAsync());

        /// <summary>
        /// Adds multiple typed effects to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
        {
            AddEffect<TEffect1>();
            return AddEffect<TEffect2>();
        }

        /// <summary>
        /// Adds multiple typed effects to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
            where TEffect3 : class, IDefaultTransitionEffect
        {
            AddEffects<TEffect1, TEffect2>();
            return AddEffect<TEffect3>();
        }

        /// <summary>
        /// Adds multiple typed effects to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        /// <typeparam name="TEffect4">The type of the fourth effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>()
            where TEffect1 : class, IDefaultTransitionEffect
            where TEffect2 : class, IDefaultTransitionEffect
            where TEffect3 : class, IDefaultTransitionEffect
            where TEffect4 : class, IDefaultTransitionEffect
        {
            AddEffects<TEffect1, TEffect2, TEffect3>();
            return AddEffect<TEffect4>();
        }

        /// <summary>
        /// Adds multiple typed effects to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        /// <typeparam name="TEffect4">The type of the fourth effect handler.</typeparam>
        /// <typeparam name="TEffect5">The type of the fifth effect handler.</typeparam>
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
