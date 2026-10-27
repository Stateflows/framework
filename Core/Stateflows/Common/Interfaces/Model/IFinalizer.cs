using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Defines an operation invoked when a behavior is finalized.</summary>
    public interface IFinalizer
    {
        /// <summary>Performs finalization work for the behavior.</summary>
        /// <returns>A task that completes when finalization work has finished.</returns>
        Task OnFinalizeAsync();
    }
}
