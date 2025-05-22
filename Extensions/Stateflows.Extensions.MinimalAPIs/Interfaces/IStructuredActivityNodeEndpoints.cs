using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

public interface IStructuredActivityNodeEndpoints : IStructuredActivityNode
{
    void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}