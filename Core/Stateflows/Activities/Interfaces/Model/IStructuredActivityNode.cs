using System.Threading.Tasks;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Marks a node that contains a nested activity graph.</summary>
    public interface IStructuredActivityNode : IActivityNode;

    /// <summary>Defines the nested graph for a structured activity node.</summary>
    public interface IStructuredActivityNodeDefinition : IStructuredActivityNode
    {
        /// <summary>Builds the nested activity graph.</summary>
        /// <param name="builder">Builder used to declare the structured graph.</param>
        static abstract void Build(IStructuredActivityBuilder builder);
    }
    
    /// <summary>Defines asynchronous initialization for a structured activity node.</summary>
    public interface IStructuredActivityNodeInitialization : IStructuredActivityNode
    {
        /// <summary>Runs when the structured node is initialized.</summary>
        /// <returns>A task representing initialization.</returns>
        Task OnInitializeAsync();
    }

    /// <summary>Defines asynchronous finalization for a structured activity node.</summary>
    public interface IStructuredActivityNodeFinalization : IStructuredActivityNode
    {
        /// <summary>Runs when the structured node is finalized.</summary>
        /// <returns>A task representing finalization.</returns>
        Task OnFinalizeAsync();
    }

    /// <summary>Combines structured-node initialization, finalization, and action behavior.</summary>
    public interface IStructuredActivityNodeAction :
        IStructuredActivityNodeInitialization,
        IStructuredActivityNodeFinalization,
        IAbstractAction
    {
        /// <inheritdoc/>
        Task IStructuredActivityNodeInitialization.OnInitializeAsync()
            => ExecuteAsync();

        /// <inheritdoc/>
        Task IStructuredActivityNodeFinalization.OnFinalizeAsync()
            => ExecuteAsync();
    }
}
