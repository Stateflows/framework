using System;
using System.Threading.Tasks;

namespace Stateflows.StateMachines.Inspection.Interfaces
{
    [Obsolete]
    public interface IStateMachineInspectionContext : IStateMachineContext
    {
        Task<IStateMachineInspection> GetInspectionAsync();
    }
}
