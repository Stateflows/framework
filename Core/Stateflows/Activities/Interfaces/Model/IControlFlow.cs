using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IControlFlow : IEdge
    { }

    public interface IControlFlowGuard : IControlFlow
    {
        Task<bool> GuardAsync();
    }
}
