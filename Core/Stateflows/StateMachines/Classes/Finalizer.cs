using System;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class Finalizer: IFinalizer
    {
        [Obsolete("Context is deprecated, use dependency injection services (IExecutionContext, IStateMachineContext, IBehaviorLocator) or value accesors (GlobalValue)")]
        public IStateMachineActionContext Context { get; internal set; }

        public abstract Task OnFinalize();
    }
}
