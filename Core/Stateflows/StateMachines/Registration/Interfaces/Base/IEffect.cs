using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Extensions;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IEffect<out TEvent, out TReturn>
    {
        /// <summary>
        /// Adds an asynchronous effect function to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>async c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="effectsAsync">Asynchronous effect functions</param>
        TReturn AddEffect(params Func<ITransitionContext<TEvent>, Task>[] effectsAsync);

        /// <summary>
        /// Adds a synchronous effect function to the current transition.<br/>
        /// Use the following pattern to implement function:
        /// <code>c => {
        ///     // function logic here; transition context is available via c parameter
        /// }</code>
        /// </summary>
        /// <param name="effects">Synchronous effect functions</param>
        [DebuggerHidden]
        public TReturn AddEffect(params Action<ITransitionContext<TEvent>>[] effects)
            => AddEffect(
                effects.Select(effect => effect
                    .AddStateMachineInvocationContext(((IEdgeBuilder)this).Edge.Graph)
                    .ToAsync()
                ).ToArray()
            );

        /// <summary>
        /// Adds a typed effect handler to the current transition.
        /// </summary>
        /// <typeparam name="TEffect">The type of the effect handler.</typeparam>
        TReturn AddEffect<TEffect>()
            where TEffect : class, ITransitionEffect<TEvent>
            => AddEffect(c => ((BaseContext)c).Context.Executor.GetTransitionEffect<TEffect, TEvent>(c)?.EffectAsync(c.Event));

        /// <summary>
        /// Adds multiple typed effect handlers to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
        {
            AddEffect<TEffect1>();
            return AddEffect<TEffect2>();
        }

        /// <summary>
        /// Adds multiple typed effect handlers to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2>();
            return AddEffect<TEffect3>();
        }

        /// <summary>
        /// Adds multiple typed effect handlers to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        /// <typeparam name="TEffect4">The type of the fourth effect handler.</typeparam>
        TReturn AddEffects<TEffect1, TEffect2, TEffect3, TEffect4>()
            where TEffect1 : class, ITransitionEffect<TEvent>
            where TEffect2 : class, ITransitionEffect<TEvent>
            where TEffect3 : class, ITransitionEffect<TEvent>
            where TEffect4 : class, ITransitionEffect<TEvent>
        {
            AddEffects<TEffect1, TEffect2, TEffect3>();
            return AddEffect<TEffect4>();
        }

        /// <summary>
        /// Adds multiple typed effect handlers to the current transition.
        /// </summary>
        /// <typeparam name="TEffect1">The type of the first effect handler.</typeparam>
        /// <typeparam name="TEffect2">The type of the second effect handler.</typeparam>
        /// <typeparam name="TEffect3">The type of the third effect handler.</typeparam>
        /// <typeparam name="TEffect4">The type of the fourth effect handler.</typeparam>
        /// <typeparam name="TEffect5">The type of the fifth effect handler.</typeparam>
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
