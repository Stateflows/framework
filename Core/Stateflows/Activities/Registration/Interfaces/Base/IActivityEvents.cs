using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivityEvents<TReturn>
    {
        TReturn AddDefaultInitializer(Func<IActivityInitializationContext, Task<bool>> actionAsync);

        TReturn AddInitializer<TInitializationEvent>(Func<IActivityInitializationContext<TInitializationEvent>, Task<bool>> actionAsync);

        TReturn AddFinalizer(Func<IActivityActionContext, Task> actionAsync);
    }
}
