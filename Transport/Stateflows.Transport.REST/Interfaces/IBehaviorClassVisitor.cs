using Microsoft.AspNetCore.Routing;

namespace Stateflows.Transport.REST;

internal interface IBehaviorClassVisitor
{
    RouteGroupBuilder GetRouteGroup(string behaviorClassName);
}