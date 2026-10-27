using System;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Composes transition predicates into conjunctions and nested guard expressions.</summary>
    /// <typeparam name="TEvent">Trigger event payload type available to the guard or effect through its execution
    /// context.</typeparam>
    /// <typeparam name="TReturn">The fluent transition builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
    public interface IGuard<TEvent, out TReturn> : IBaseGuard<TEvent, TReturn>
    {
        /// <summary>Adds a composed predicate to the current transition.</summary>
        /// <param name="guardExpression">Callback adding predicates and nested Boolean expressions. Predicates at the root
        /// are combined with AND.</param>
        /// <returns>The current transition builder after adding the composed predicate.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddGuardExpression(Func<IGuardBuilder<TEvent>, IGuardBuilder<TEvent>> guardExpression)
        {
            var builder = new GuardBuilder<TEvent>(((IVertexBuilder)this).Vertex);
            guardExpression.Invoke(builder);

            return AddGuard(builder.GetAndGuard());
        }

        /// <summary>Registers typed predicates for the current transition.</summary>
        /// <typeparam name="TGuard1">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard2">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <returns>The current transition builder after adding the guard conjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddGuards<TGuard1, TGuard2>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
            );

        /// <summary>Registers typed predicates for the current transition.</summary>
        /// <typeparam name="TGuard1">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard2">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard3">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <returns>The current transition builder after adding the guard conjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        TReturn AddGuards<TGuard1, TGuard2, TGuard3>()
            where TGuard1 : class, ITransitionGuard<TEvent>
            where TGuard2 : class, ITransitionGuard<TEvent>
            where TGuard3 : class, ITransitionGuard<TEvent>
            => AddGuardExpression(b => b
                .AddGuard<TGuard1>()
                .AddGuard<TGuard2>()
                .AddGuard<TGuard3>()
            );

        /// <summary>Registers typed predicates for the current transition.</summary>
        /// <typeparam name="TGuard1">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard2">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard3">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard4">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <returns>The current transition builder after adding the guard conjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
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

        /// <summary>Registers typed predicates for the current transition.</summary>
        /// <typeparam name="TGuard1">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard2">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard3">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard4">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <typeparam name="TGuard5">Typed guard implementing <see cref="ITransitionGuard&lt;TEvent&gt;" />. Its result
        /// must be true for this predicate to accept.</typeparam>
        /// <returns>The current transition builder after adding the guard conjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
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
