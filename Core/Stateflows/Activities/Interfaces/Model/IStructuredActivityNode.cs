using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IBaseStructuredActivityNode : IActivityNode
    { }

    public interface IStructuredActivityNodeInitialization : IBaseStructuredActivityNode
    {
        Task OnInitializeAsync();
    }

    public interface IStructuredActivityNodeFinalization : IBaseStructuredActivityNode
    {
        Task OnFinalizeAsync();
    }
}
