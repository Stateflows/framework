using Microsoft.AspNetCore.Routing;

namespace Stateflows.Extensions.MinimalAPIs;

internal interface IBehaviorClassVisitor
{
    RouteGroupBuilder GetRouteGroup(string behaviorClassName);
    void AddLink(HateoasLink link, string scope = "");
    Dictionary<string, List<HateoasLink>> CustomHateoasLinks { get; }
}