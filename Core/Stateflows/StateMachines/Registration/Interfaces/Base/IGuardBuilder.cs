using System;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Builds a transition guard expression, including nested Boolean combinations.</summary>
    /// <typeparam name="TEvent">Trigger event payload type available to the guard or effect through its execution
    /// context.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
    public interface IGuardBuilder<TEvent> : IBaseGuard<TEvent, IGuardBuilder<TEvent>>
    {
        /// <summary>Adds a nested conjunction to the guard expression.</summary>
        /// <param name="guardExpressionBuilder">Callback populating the nested group; all predicates must accept.</param>
        /// <returns>This expression builder after adding the nested conjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        IGuardBuilder<TEvent> AddAndExpression(Action<IGuardBuilder<TEvent>> guardExpressionBuilder);

        /// <summary>Adds a nested disjunction to the guard expression.</summary>
        /// <param name="guardExpressionBuilder">Callback populating the nested group; at least one predicate must accept.
        /// </param>
        /// <returns>This expression builder after adding the nested disjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        IGuardBuilder<TEvent> AddOrExpression(Action<IGuardBuilder<TEvent>> guardExpressionBuilder);
    }
}