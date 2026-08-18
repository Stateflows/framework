using Stateflows.Common;

namespace Stateflows.Filters;

public class ValidationFilter(IEnumerable<IStateflowsValidator> validators) : IIncomingGrainCallFilter
{
    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (context.Grain is BehaviorGrain)
        {
            var eventHolder = (EventHolder)context.Request.GetArgument(0)!;
            var validation = await eventHolder.ValidateAsync(validators.ToArray());
            if (!validation.IsValid)
            {
                context.Result = (OrleansRequestResult)new RequestResult(null, EventStatus.Invalid, validation);
                return;
            }
        }

        await context.Invoke();
    }
}