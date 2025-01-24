using System;
using Stateflows.StateMachines.Context.Classes;
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
            var builder = new GuardBuilder<TEvent>(this as IInternal, ((IEdgeBuilder)this).Edge);
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
        /// Adds multiple typed guard handlers to the current transition using AND logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);

                return await guard1.GuardAsync(c.Event) && await guard2.GuardAsync(c.Event);
            });

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
        /// Adds multiple typed guard handlers to the current transition using AND logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);

                return await guard1.GuardAsync(c.Event) && await guard2.GuardAsync(c.Event) && await guard3.GuardAsync(c.Event);
            });

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
        /// Adds multiple typed guard handlers to the current transition using AND logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        /// <typeparam name="TGuard4">The type of the fourth guard handler.</typeparam>
        [Obsolete("AddAndGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddAndGuards<TGuard1, TGuard2, TGuard3, TGuard4>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);
                var guard4 = executor.GetTransitionGuard<TGuard4, TEvent>(c);

                return await guard1.GuardAsync(c.Event) && await guard2.GuardAsync(c.Event) && await guard3.GuardAsync(c.Event) && await guard4.GuardAsync(c.Event);
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
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            where TGuard5 : class, ITransitionGuard<TEvent>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);
                var guard4 = executor.GetTransitionGuard<TGuard4, TEvent>(c);
                var guard5 = executor.GetTransitionGuard<TGuard5, TEvent>(c);

                return await guard1.GuardAsync(c.Event) && await guard2.GuardAsync(c.Event) && await guard3.GuardAsync(c.Event) && await guard4.GuardAsync(c.Event) && await guard5.GuardAsync(c.Event);
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using OR logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);

                return await guard1.GuardAsync(c.Event) || await guard2.GuardAsync(c.Event);
            });

        /// <summary>
        /// Adds multiple typed guard handlers to the current transition using OR logic.
        /// </summary>
        /// <typeparam name="TGuard1">The type of the first guard handler.</typeparam>
        /// <typeparam name="TGuard2">The type of the second guard handler.</typeparam>
        /// <typeparam name="TGuard3">The type of the third guard handler.</typeparam>
        [Obsolete("AddOrGuards method is obsolete. Use AddGuardExpression instead")]
        TReturn AddOrGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);

                return await guard1.GuardAsync(c.Event) || await guard2.GuardAsync(c.Event) || await guard3.GuardAsync(c.Event);
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
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);
                var guard4 = executor.GetTransitionGuard<TGuard4, TEvent>(c);

                return await guard1.GuardAsync(c.Event) || await guard2.GuardAsync(c.Event) || await guard3.GuardAsync(c.Event) || await guard4.GuardAsync(c.Event);
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
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            where TGuard4 : class, ITransitionGuard<TEvent>
            where TGuard5 : class, ITransitionGuard<TEvent>
            => AddGuard(async c =>
            {
                var executor = ((BaseContext)c).Context.Executor;
                var guard1 = executor.GetTransitionGuard<TGuard1, TEvent>(c);
                var guard2 = executor.GetTransitionGuard<TGuard2, TEvent>(c);
                var guard3 = executor.GetTransitionGuard<TGuard3, TEvent>(c);
                var guard4 = executor.GetTransitionGuard<TGuard4, TEvent>(c);
                var guard5 = executor.GetTransitionGuard<TGuard5, TEvent>(c);

                return await guard1.GuardAsync(c.Event) || await guard2.GuardAsync(c.Event) || await guard3.GuardAsync(c.Event) || await guard4.GuardAsync(c.Event) || await guard5.GuardAsync(c.Event);
            });
    }
}
