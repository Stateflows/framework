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
    private IGrainTimer? Timer;
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
            
            Timer = this.RegisterGrainTimer(
                static (state, ct) => state.CancelAsync(),
                this,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromMinutes(1)
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return base.OnActivateAsync(cancellationToken);
    }

    public async Task CancelAsync()
    {
        if (Common.Context.Classes.BaseContext.Instances.TryGetValue(BehaviorId, out var contexts))
        {
            foreach (var context in contexts)
            {
                await context.CancellationTokenSource.CancelAsync();
            }
        }
    }

    public async Task<OrleansRequestResult> ProcessEventAsync(OrleansEventHolder orleansEventHolder, CancellationToken cancellationToken)
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
                if (validation.IsValid)
                {
                    status = await eventHolder.ExecuteAsync(this);
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