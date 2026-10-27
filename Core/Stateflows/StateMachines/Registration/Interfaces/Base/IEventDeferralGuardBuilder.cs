using System;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Builds a deferral guard expression, including nested Boolean combinations.</summary>
    /// <typeparam name="TEvent">Deferred event payload type.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
    public interface IEventDeferralGuardBuilder<TEvent> : IBaseDeferralGuard<TEvent, IEventDeferralGuardBuilder<TEvent>>
    {
        /// <summary>Adds a nested conjunction to the guard expression.</summary>
        /// <param name="guardExpressionBuilder">Callback populating the nested group; all predicates must accept.</param>
        /// <returns>This expression builder after adding the nested conjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        IEventDeferralGuardBuilder<TEvent> AddAndExpression(Action<IEventDeferralGuardBuilder<TEvent>> guardExpressionBuilder);

        /// <summary>Adds a nested disjunction to the guard expression.</summary>
        /// <param name="guardExpressionBuilder">Callback populating the nested group; at least one predicate must accept.
        /// </param>
        /// <returns>This expression builder after adding the nested disjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-deferring-events" />
        IEventDeferralGuardBuilder<TEvent> AddOrExpression(Action<IEventDeferralGuardBuilder<TEvent>> guardExpressionBuilder);
    }
}