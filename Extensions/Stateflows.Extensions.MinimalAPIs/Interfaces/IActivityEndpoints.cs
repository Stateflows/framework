using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

public interface IActivityEndpoints : IActivity
{
    void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}