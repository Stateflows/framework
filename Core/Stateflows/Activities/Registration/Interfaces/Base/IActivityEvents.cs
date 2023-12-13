using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivityEvents<TReturn>
    {
        TReturn AddOnInitialize(Func<IActivityInitializationContext, Task<bool>> actionAsync);

        TReturn AddOnInitialize<TInitializationRequest>(Func<IActivityInitializationContext<TInitializationRequest>, Task<bool>> actionAsync)
            where TInitializationRequest : InitializationRequest, new();

        TReturn AddOnFinalize(Func<IActivityActionContext, Task> actionAsync);
    }
}
