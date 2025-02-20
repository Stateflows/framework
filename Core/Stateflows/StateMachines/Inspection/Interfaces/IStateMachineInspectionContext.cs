using System.Threading.Tasks;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    public interface IStateMachineInspectionContext : IStateMachineContext
    {
        Task<IStateMachineInspection> GetInspectionAsync();
    }
}
