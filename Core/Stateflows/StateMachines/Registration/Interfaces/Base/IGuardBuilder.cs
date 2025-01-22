using System;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IGuardBuilder<out TEvent> : IBaseGuard<TEvent, IGuardBuilder<TEvent>>
    {
        IGuardBuilder<TEvent> AddAndExpression(Action<IGuardBuilder<TEvent>> guardExpression);
        IGuardBuilder<TEvent> AddOrExpression(Action<IGuardBuilder<TEvent>> guardExpression);
    }
}