using System.Threading.Tasks;

namespace Stateflows.StateMachines
{
    public interface IStateMachineContextProvider
    {
        Task<(bool Success, IStateMachineContextHolder ContextHolder)> TryProvideAsync(StateMachineId stateMachineId);
    }
}