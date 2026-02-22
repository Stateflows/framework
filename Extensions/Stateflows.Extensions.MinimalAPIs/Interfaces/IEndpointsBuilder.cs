using Microsoft.AspNetCore.Routing;

namespace Stateflows.Extensions.MinimalAPIs;

public interface IEndpointsBuilder
{
    RouteGroupBuilder AddGet(string pattern, Delegate handler);
    RouteGroupBuilder AddPost(string pattern, Delegate handler);
    RouteGroupBuilder AddPatch(string pattern, Delegate handler);
    RouteGroupBuilder AddPut(string pattern, Delegate handler);
    RouteGroupBuilder AddDelete(string pattern, Delegate handler);
    RouteGroupBuilder AddMethods(string pattern, string[] methods, Delegate handler);
}