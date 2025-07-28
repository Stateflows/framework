using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

public static class HateoasLinksExtensions
{
    public static void AddLink(this Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> hateoasLinks, string behaviorName, HateoasLink link, BehaviorStatus[] supportedStatuses, string scope = "", string scopeKind = "")
    {
        scope = string.IsNullOrEmpty(scope) ? $"{behaviorName}" : $"{behaviorName}:{scopeKind}:{scope}";
        if (!hateoasLinks.TryGetValue(scope, out var links))
        {
            links = new List<(HateoasLink, BehaviorStatus[])>();
            hateoasLinks.Add(scope, links);
        }
        
        links.Add((link, supportedStatuses));
    }
}