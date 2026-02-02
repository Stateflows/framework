using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Interfaces;

public interface IAbstractAction
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}