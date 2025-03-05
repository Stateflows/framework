using Microsoft.AspNetCore.Builder;

namespace Stateflows.Transport.REST;

public interface IEndpointsBuilder
{
    RouteHandlerBuilder AddGet(string pattern, Delegate handler);        
    RouteHandlerBuilder AddPost(string pattern, Delegate handler);        
    RouteHandlerBuilder AddPatch(string pattern, Delegate handler);        
    RouteHandlerBuilder AddPut(string pattern, Delegate handler);        
    RouteHandlerBuilder AddDelete(string pattern, Delegate handler);        
    RouteHandlerBuilder AddMethods(string pattern, string[] methods, Delegate handler);
}