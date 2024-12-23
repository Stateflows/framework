using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IBaseGuard<out TEvent, out TReturn>
    {
        TReturn AddGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync);

        TReturn AddNegatedGuard(Func<ITransitionContext<TEvent>, Task<bool>> guardAsync)
            => AddGuard(async c => !await guardAsync.Invoke(c));

        TReturn AddGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard>();

            return AddGuard(c => (c as BaseContext).Context.Executor.GetTransitionGuard<TGuard, TEvent>(c)?.GuardAsync(c.Event));
        }
        
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, ITransitionGuard<TEvent>
        {
            (this as IInternal).Services.AddServiceType<TGuard>();

            return AddGuard(async c => !await (c as BaseContext).Context.Executor.GetTransitionGuard<TGuard, TEvent>(c)?.GuardAsync(c.Event));
        }
    }
}
