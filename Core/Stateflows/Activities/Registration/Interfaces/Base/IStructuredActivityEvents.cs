using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IStructuredActivityEvents<out TReturn>
    {
        TReturn AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync);

        TReturn AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync);
    }
}
