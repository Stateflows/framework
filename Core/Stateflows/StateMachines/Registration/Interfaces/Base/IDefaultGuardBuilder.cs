using System;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDefaultGuardBuilder : IBaseDefaultGuard<IDefaultGuardBuilder>
    {
        /// <summary>
        /// Adds a <i>AND</i> guard expression to the current transition.<br/>
        /// All subexpressions added to it must be true for the transition to be taken.
        /// </summary>
        /// <param name="guardExpressionBuilder">Guard expression builder</param>
        IDefaultGuardBuilder AddAndExpression(Action<IDefaultGuardBuilder> guardExpressionBuilder);

        /// <summary>
        /// Adds a <i>OR</i> guard expression to the current transition.<br/>
        /// At least one subexpression added to it must be true for the transition to be taken.
        /// </summary>
        /// <param name="guardExpressionBuilder">Guard expression builder</param>
        IDefaultGuardBuilder AddOrExpression(Action<IDefaultGuardBuilder> guardExpressionBuilder);
    }
}