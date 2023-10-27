using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IStructuredActivityEvents<TReturn>
    {
        TReturn AddOnInitialize(Func<IActivityActionContext, Task> actionAsync);

        TReturn AddOnFinalize(Func<IActivityActionContext, Task> actionAsync);
    }
}
