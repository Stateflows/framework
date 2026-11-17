using System.Threading.Tasks;

namespace Stateflows.Common;

/// <summary>Infrastructure for delivering an already-intercepted request envelope without regenerating its identity.</summary>
public interface IEnvelopeRequestBehavior
{
    Task<RequestResult<TResponse>> RequestEnvelopeAsync<TResponse>(EventHolder envelope);
}
