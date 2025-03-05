using Stateflows.Transport.REST;

namespace Stateflows.Activities;

public interface IActivityEndpoints : IActivity
{
    void RegisterEndpoints(IEndpointsBuilder endpointsBuilder);
}