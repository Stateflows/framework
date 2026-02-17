using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stateflows.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Utilities;
using Stateflows.Common.Interfaces;

namespace Stateflows;

internal class BehaviorGrain( 
    IGrainFactory grainFactory,
    IStateflowsInterceptor interceptor,
    IEnumerable<IStateflowsValidator> validators,
    IEnumerable<IEventProcessor> processors,
    IStateflowsTenantExecutor tenantExecutor
) : Grain, IBehaviorGrain, IRemindable, IStateflowsExecutor
{
    private string? TenantId;
    private BehaviorId BehaviorId;
    private readonly Dictionary<string, IEventProcessor> Processors = processors.ToDictionary(p => p.BehaviorType, p => p);
    private readonly IStateflowsValidator[] Validators = validators.ToArray();
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
        
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task<OrleansRequestResult> ProcessEventAsync(OrleansEventHolder orleansEventHolder)
    {
        var status = EventStatus.Invalid;
        var responses = new Dictionary<object, EventHolder>();
        var eventHolder = (EventHolder)orleansEventHolder;

        ResponseHolder.SetResponses(responses);
        var validation = await eventHolder.ValidateAsync(Validators);
        
        await tenantExecutor.ExecuteByTenantAsync(TenantId, async () =>
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

        if (result.Response != null)
        {
            var notificationType = result.Response.PayloadType;
            var ttlAttribute = notificationType.GetCustomAttribute<TimeToLiveAttribute>();
            var retainAttribute = notificationType.GetCustomAttribute<RetainAttribute>();
            var notification = (OrleansEventHolder)result.Response;
            notification.SenderId = BehaviorId;
            notification.SentAt = DateTime.Now;
            notification.Retained = retainAttribute != null;
            notification.TimeToLive = ttlAttribute?.SecondsToLive ?? 0;
            
            var notificationsGrain = grainFactory.GetGrain<INotificationsGrain>(this.GetGrainId().Key.ToString());
            await notificationsGrain.PublishAsync([notification]);
        }
        
        ResponseHolder.ClearResponses();

        return result;
    }

    public async Task<EventStatus> DoProcessAsync<TEvent>(EventHolder<TEvent> eventHolder)
    {
        var result = EventStatus.Undelivered;
        if (!Processors.TryGetValue(BehaviorId.Type, out var processor) || !interceptor.BeforeExecute(BehaviorId, eventHolder))
        {
            return result;
        }
        
        var exceptions = new List<Exception>();

        result = await processor.ProcessEventAsync(BehaviorId, eventHolder, exceptions);
        
        return result;
    }

    public Task ReceiveReminder(string reminderName, TickStatus status)
    {
        throw new NotImplementedException();
    }
}