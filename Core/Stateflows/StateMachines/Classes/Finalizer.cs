using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class Finalizer
    {
        public IStateMachineActionContext Context { get; internal set; }

        public abstract Task<bool> OnFinalize();
    }
}
