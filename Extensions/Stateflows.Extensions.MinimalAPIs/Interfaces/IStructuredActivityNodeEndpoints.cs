using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

public interface IStructuredActivityNodeEndpoints : IStructuredActivityNode
{
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}