using System.Threading.Tasks;

namespace Stateflows.Activities.Inspection.Interfaces
{
    public interface IActivityInspectionContext : IActivityContext
    {
        Task<IActivityInspection> GetInspectionAsync();
    }
}
