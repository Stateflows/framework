using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public class FinalState : BaseState, IFinalState
    {
        public const string Name = "Stateflows.StateMachines.FinalState";

        public sealed override Task OnEntryAsync()
            => Task.CompletedTask;

        public sealed override Task OnExitAsync()
            => Task.CompletedTask;
    }
}
