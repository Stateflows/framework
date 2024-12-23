using System;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IDefaultGuardBuilder : IBaseDefaultGuard<IDefaultGuardBuilder>
    {
        IDefaultGuardBuilder AddAndExpression(Action<IDefaultGuardBuilder> guardExpression);
        IDefaultGuardBuilder AddOrExpression(Action<IDefaultGuardBuilder> guardExpression);
    }
}