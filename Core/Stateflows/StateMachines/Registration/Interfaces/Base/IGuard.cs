using System;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IGuard<TEvent, out TReturn> : IBaseGuard<TEvent, TReturn>
    {
        /// <summary>
        /// Adds a guard expression to the current transition.
        /// </summary>
        /// <param name="guardExpression">The guard expression to add.</param>
        TReturn AddGuardExpression(Func<IGuardBuilder<TEvent>, IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(((IEdgeBuilder)this).Edge);
            guardExpression.Invoke(builder);

            return AddGuard(builder.GetAndGuard());
        }

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
            );

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
            );

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
                .AddGuard<TGuard4>()
            );

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        /// <typeparam name="TGuard5">The type of the fifth guard handler.</typeparam>
        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            where TGuard5 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
                .AddGuard<TGuard4>()
                .AddGuard<TGuard5>()
            );
    }
}
