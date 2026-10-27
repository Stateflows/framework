using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>Collects behavior operation links by behavior and optional state/node scope.</summary>
public static class HateoasLinksExtensions
{
    /// <summary>Adds an operation link with the lifecycle statuses in which it is available.</summary>
    /// <param name="hateoasLinks">Registry receiving the link.</param>
    /// <param name="behaviorName">Registered behavior class name.</param>
    /// <param name="link">HTTP operation link.</param>
    /// <param name="supportedStatuses">Lifecycle statuses for which the link is applicable.</param>
    /// <param name="scope">Optional state or node name; empty means behavior-wide.</param>
    /// <param name="scopeKind">Scope discriminator used with a nonempty scope.</param>
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