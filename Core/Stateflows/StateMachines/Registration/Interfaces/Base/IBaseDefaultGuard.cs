using System;
using System.Threading.Tasks;
using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Events;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IBaseDefaultGuard<out TReturn>
    {
        TReturn AddGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync);

        TReturn AddNegatedGuard(Func<ITransitionContext<Completion>, Task<bool>> guardAsync)
            => AddGuard(async c => !await guardAsync.Invoke(c));
        
        TReturn AddGuard<TGuard>()
            where TGuard : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard>();

            return AddGuard(c => (c as BaseContext).Context.Executor.GetDefaultTransitionGuard<TGuard>(c)?.GuardAsync());
        }
        
        TReturn AddNegatedGuard<TGuard>()
            where TGuard : class, IDefaultTransitionGuard
        {
            (this as IInternal).Services.AddServiceType<TGuard>();

            return AddGuard(async c => !await (c as BaseContext).Context.Executor.GetDefaultTransitionGuard<TGuard>(c)?.GuardAsync());
        }
    }
}
