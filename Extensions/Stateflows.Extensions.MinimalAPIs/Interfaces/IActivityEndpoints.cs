using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Activities;

public interface IActivityEndpoints : IActivity
{
    static abstract void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}