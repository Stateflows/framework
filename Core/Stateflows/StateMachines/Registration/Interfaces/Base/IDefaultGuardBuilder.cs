using System;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Builds a completion guard expression, including nested Boolean combinations.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
    public interface IDefaultGuardBuilder : IBaseDefaultGuard<IDefaultGuardBuilder>
    {
        /// <summary>Adds a nested conjunction to the guard expression.</summary>
        /// <param name="guardExpressionBuilder">Callback populating the nested group; all predicates must accept.</param>
        /// <returns>This expression builder after adding the nested conjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        IDefaultGuardBuilder AddAndExpression(Action<IDefaultGuardBuilder> guardExpressionBuilder);

        /// <summary>Adds a nested disjunction to the guard expression.</summary>
        /// <param name="guardExpressionBuilder">Callback populating the nested group; at least one predicate must accept.
        /// </param>
        /// <returns>This expression builder after adding the nested disjunction.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-transitions#logic" />
        IDefaultGuardBuilder AddOrExpression(Action<IDefaultGuardBuilder> guardExpressionBuilder);
    }
}