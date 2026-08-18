using Stateflows.Common.Engine.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Filters;

public class ResourceFilter(IStateflowsTelemetry telemetry) : IIncomingGrainCallFilter
{
    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (context.Grain is not BehaviorGrain)
        {
            await context.Invoke();
        }
        
        var tenantBehaviorId = StateflowsJsonConverter.DeserializeObject<TenantBehaviorId>(context.TargetContext.GrainId.Key.ToString());

        if (!telemetry.ResourcesByBehaviorClass.TryGetValue(tenantBehaviorId.BehaviorId.BehaviorClass, out var resource))
        {
            return;
        }
            
        using var awaiter = await resource.AcquireAsync();

        try
        {
            await context.Invoke();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}