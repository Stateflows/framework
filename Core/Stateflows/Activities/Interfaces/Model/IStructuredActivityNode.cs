using System.Threading.Tasks;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IStructuredActivityNode : IActivityNode;

    public interface IStructuredActivityNodeDefinition : IStructuredActivityNode
    {
        static abstract void Build(IStructuredActivityBuilder builder);
    }
    
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
