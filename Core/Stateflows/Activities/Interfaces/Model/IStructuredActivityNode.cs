using System.Threading.Tasks;

namespace Stateflows.Activities
{
    public interface IStructuredActivityNode
    { }

    public interface IStructuredActivityNodeInitialization : IStructuredActivityNode
    {
        Task OnInitializeAsync();
    }

    public interface IStructuredActivityNodeFinalization : IStructuredActivityNode
    {
        Task OnFinalizeAsync();
    }
}
