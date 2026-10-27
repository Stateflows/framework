using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

/// <summary>Defines an asynchronous executable operation with optional cooperative cancellation.</summary>
public interface IAbstractAction
{
    /// <summary>Executes the operation.</summary>
    /// <param name="cancellationToken">A token that allows the operation to observe cancellation.</param>
    /// <returns>A task that completes when the operation has finished.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}