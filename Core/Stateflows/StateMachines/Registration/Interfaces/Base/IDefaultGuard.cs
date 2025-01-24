using System;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDefaultGuard<out TReturn> : IBaseDefaultGuard<TReturn>
    {
        /// <summary>
        /// Adds a guard expression to the current transition.
        /// </summary>
        /// <param name="guardExpression">The guard expression to add.</param>
        TReturn AddGuardExpression(Func<IDefaultGuardBuilder, IDefaultGuardBuilder> guardExpression)
        {
            var builder = new GuardBuilder<Completion>(this as IInternal, ((IEdgeBuilder)this).Edge);
            guardExpression.Invoke(builder);

            return AddGuard(builder.GetAndGuard());
        }

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
            );

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using AND logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync();
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
            );

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using AND logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync();
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
                .AddGuard<TGuard4>()
            );

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using AND logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);
                var guard4 = executor.GetDefaultTransitionGuard<TGuard4>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync() && await guard4.GuardAsync();
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        /// <typeparam name="TGuard5">The type of the fifth guard handler.</typeparam>
        TReturn AddGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            where TGuard5 : class, IDefaultTransitionGuard
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
                .AddGuard<TGuard4>()
                .AddGuard<TGuard5>()
            );

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using AND logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        /// <typeparam name="TGuard5">The type of the fifth guard handler.</typeparam>
        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            where TGuard5 : class, IDefaultTransitionGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);
                var guard4 = executor.GetDefaultTransitionGuard<TGuard4>(c);
                var guard5 = executor.GetDefaultTransitionGuard<TGuard5>(c);

                return await guard1.GuardAsync() && await guard2.GuardAsync() && await guard3.GuardAsync() && await guard4.GuardAsync() && await guard5.GuardAsync();
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using OR logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync();
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using OR logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync();
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using OR logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);
                var guard4 = executor.GetDefaultTransitionGuard<TGuard4>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync() || await guard4.GuardAsync();
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using OR logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        /// <typeparam name="TGuard5">The type of the fifth guard handler.</typeparam>
        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3, TGuard4, TGuard5>()
            where TGuard1 : class, IDefaultTransitionGuard
            where TGuard2 : class, IDefaultTransitionGuard
            where TGuard3 : class, IDefaultTransitionGuard
            where TGuard4 : class, IDefaultTransitionGuard
            where TGuard5 : class, IDefaultTransitionGuard
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetDefaultTransitionGuard<TGuard1>(c);
                var guard2 = executor.GetDefaultTransitionGuard<TGuard2>(c);
                var guard3 = executor.GetDefaultTransitionGuard<TGuard3>(c);
                var guard4 = executor.GetDefaultTransitionGuard<TGuard4>(c);
                var guard5 = executor.GetDefaultTransitionGuard<TGuard5>(c);

                return await guard1.GuardAsync() || await guard2.GuardAsync() || await guard3.GuardAsync() || await guard4.GuardAsync() || await guard5.GuardAsync();
            });
    }
}
