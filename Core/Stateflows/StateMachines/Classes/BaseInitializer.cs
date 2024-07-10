using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public abstract class BaseInitializer
    {
        public IStateMachineInitializationContext Context { get; internal set; }
    }
}
