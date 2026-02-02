using System.Threading.Tasks;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IStructuredActivityNode : IActivityNode;

    public interface IStructuredActivityNodeInitialization : IStructuredActivityNode
    {
        Task OnInitializeAsync();
    }

    public interface IStructuredActivityNodeFinalization : IStructuredActivityNode
    {
        Task OnFinalizeAsync();
    }

    public interface IStructuredActivityNodeAction :
        IStructuredActivityNodeInitialization,
        IStructuredActivityNodeFinalization,
        IAbstractAction
    {
        Task IStructuredActivityNodeInitialization.OnInitializeAsync()
            => ExecuteAsync();

        Task IStructuredActivityNodeFinalization.OnFinalizeAsync()
            => ExecuteAsync();
    }
}
