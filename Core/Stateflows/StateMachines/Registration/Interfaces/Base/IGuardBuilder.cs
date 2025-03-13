using System;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IGuardBuilder<TEvent> : IBaseGuard<TEvent, IGuardBuilder<TEvent>>
    {
        /// <summary>
        /// Adds a <i>AND</i> guard expression to the current transition.<br/>
        /// All subexpressions added to it must be true for the transition to be taken.
        /// </summary>
        /// <param name="guardExpressionBuilder">Guard expression builder</param>
        IGuardBuilder<TEvent> AddAndExpression(Action<IGuardBuilder<TEvent>> guardExpressionBuilder);

        /// <summary>
        /// Adds a <i>OR</i> guard expression to the current transition.<br/>
        /// At least one subexpression added to it must be true for the transition to be taken.
        /// </summary>
        /// <param name="guardExpressionBuilder">Guard expression builder</param>
        IGuardBuilder<TEvent> AddOrExpression(Action<IGuardBuilder<TEvent>> guardExpressionBuilder);
    }
}