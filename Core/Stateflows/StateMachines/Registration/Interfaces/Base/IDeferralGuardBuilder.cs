using System;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDeferralGuardBuilder<TEvent> : IBaseDeferralGuard<TEvent, IDeferralGuardBuilder<TEvent>>
    {
        /// <summary>
        /// Adds a <i>AND</i> guard expression to the current deferral.<br/>
        /// All subexpressions added to it must be true for the deferral to be done.
        /// </summary>
        /// <param name="guardExpressionBuilder">Guard expression builder</param>
        IDeferralGuardBuilder<TEvent> AddAndExpression(Action<IDeferralGuardBuilder<TEvent>> guardExpressionBuilder);

        /// <summary>
        /// Adds a <i>OR</i> guard expression to the current deferral.<br/>
        /// At least one subexpression added to it must be true for the deferral to be done.
        /// </summary>
        /// <param name="guardExpressionBuilder">Guard expression builder</param>
        IDeferralGuardBuilder<TEvent> AddOrExpression(Action<IDeferralGuardBuilder<TEvent>> guardExpressionBuilder);
    }
}