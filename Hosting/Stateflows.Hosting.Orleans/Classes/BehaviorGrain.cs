using Microsoft.Extensions.DependencyInjection;
using Stateflows.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows;

public class BehaviorGrain(IServiceProvider serviceProvider) : Grain, IBehaviorGrain, IStateflowsExecutor
{
    private string TenantId;
    private BehaviorId BehaviorId;
    private IStateflowsInterceptor? Interceptor;
    private ITenantAccessor? TenantAccessor;
    private IStateflowsValueStorage? ValueStorage;
    private IStateflowsValidator[]? Validators;
    private Dictionary<string, IEventProcessor>? Processors;
    private IStateflowsTenantExecutor? TenantExecutor;
    
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var tenantBehaviorId = StateflowsJsonConverter.DeserializeObject<TenantBehaviorId>(this.GetGrainId().Key.ToString());
            TenantId = tenantBehaviorId.TenantId;
            BehaviorId = tenantBehaviorId.BehaviorId;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        Interceptor = serviceProvider.GetRequiredService<IStateflowsInterceptor>();
        TenantExecutor = serviceProvider.GetRequiredService<IStateflowsTenantExecutor>();
        Validators = serviceProvider.GetRequiredService<IEnumerable<IStateflowsValidator>>().ToArray();
        Processors = serviceProvider.GetRequiredService<IEnumerable<IEventProcessor>>().ToDictionary(p => p.BehaviorType, p => p);
        
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<string> ProcessAsync(string serializedEventHolder)
    {
        var status = EventStatus.Invalid;
        var eventHolder = (EventHolder)StateflowsJsonConverter.DeserializeObject(serializedEventHolder);
        var responses = new Dictionary<object, EventHolder>();

        ResponseHolder.SetResponses(responses);
        var validation = await eventHolder.ValidateAsync(Validators);
        
        await TenantExecutor.ExecuteByTenantAsync(TenantId, async () =>
        {
            try
            {
                if (
                    validation.IsValid ||
                    (
                        eventHolder is EventHolder<CompoundRequest> compoundRequest &&
                        compoundRequest.Payload.Events.Any(ev => ev.Headers.Values.Any(h => h is ForcedExecution))
                    )
                )
                {
                    status = await eventHolder.DoProcessAsync(this);
                }

                status = validation.IsValid
                    ? status
                    : EventStatus.Invalid;
            }
            catch (Exception)
            {
                status = EventStatus.Failed;
            }
        });

        var result = new RequestResult(eventHolder.GetResponseHolder(), status, validation);
        
        ResponseHolder.ClearResponses();

        return StateflowsJsonConverter.SerializePolymorphicObject(result);
    }

    public async Task<OrleansRequestResult> ProcessEventAsync(OrleansEventHolder orleansEventHolder)
    {
        var status = EventStatus.Invalid;
        var responses = new Dictionary<object, EventHolder>();
        var eventHolder = (EventHolder)orleansEventHolder;

        ResponseHolder.SetResponses(responses);
        var validation = await eventHolder.ValidateAsync(Validators);
        
        await TenantExecutor.ExecuteByTenantAsync(TenantId, async () =>
        {
            try
            {
                if (
                    validation.IsValid ||
                    (
                        eventHolder is EventHolder<CompoundRequest> compoundRequest &&
                        compoundRequest.Payload.Events.Any(ev => ev.Headers.Values.Any(h => h is ForcedExecution))
                    )
                )
                {
                    status = await eventHolder.DoProcessAsync(this);
                }

                status = validation.IsValid
                    ? status
                    : EventStatus.Invalid;
            }
            catch (Exception)
            {
                status = EventStatus.Failed;
            }
        });

        var result = new RequestResult(eventHolder.GetResponseHolder(), status, validation);
        
        ResponseHolder.ClearResponses();

        return result;
    }

    public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
    {
        var result = EventStatus.Undelivered;
        if (!Processors.TryGetValue(BehaviorId.Type, out var processor) || !Interceptor.BeforeExecute(BehaviorId, eventHolder))
        {
            return result;
        }
        
        var exceptions = new List<Exception>();

        result = await processor.ProcessEventAsync(BehaviorId, eventHolder, exceptions);
        return result;
    }
}