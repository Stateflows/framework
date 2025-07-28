using Stateflows.Common;

namespace Stateflows.Extensions.MinimalAPIs;

internal interface IBehaviorClassVisitor
{
    Dictionary<string, List<(HateoasLink, BehaviorStatus[])>> HateoasLinks { get; }
}