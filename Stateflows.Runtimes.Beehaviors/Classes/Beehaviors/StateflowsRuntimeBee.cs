using Beehaviors.Context.Interfaces;
using Beehaviors.Interfaces;
using Beehaviors.Registration.Interfaces;
using Microsoft.Extensions.Logging;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Storage;

namespace Stateflows;

internal sealed class StateflowsRuntimeBee : IBee<BeeStateflowsState>
{
    public const string BeeName = "stateflows.runtime.behavior";
    private const string ForceFinalizeKey = "Stateflows::System::Finalize";

    private readonly IStateflowsValidator[] validators;
    private readonly Dictionary<string, IEventProcessor> processorsByType;
    private readonly IStateflowsInterceptor interceptor;
    private readonly IStateflowsTenantExecutor tenantExecutor;
    private readonly IStateflowsLock stateflowsLock;
    private readonly IStateflowsValueStorage valueStorage;
    private readonly ILogger<StateflowsRuntimeBee> logger;
    private readonly RuntimeEngine runtimeEngine;

    public StateflowsRuntimeBee(
        IEnumerable<IStateflowsValidator> validators,
        IEnumerable<IEventProcessor> processors,
        IStateflowsInterceptor interceptor,
        IStateflowsTenantExecutor tenantExecutor,
        IStateflowsLock stateflowsLock,
        IStateflowsValueStorage valueStorage,
        ILogger<StateflowsRuntimeBee> logger
    )
    {
        this.validators = validators.ToArray();
        processorsByType = processors
            .SelectMany(processor => processor.BehaviorTypes.Select(type => new { Type = type, Processor = processor }))
            .ToDictionary(pair => pair.Type, pair => pair.Processor);
        this.interceptor = interceptor;
        this.tenantExecutor = tenantExecutor;
        this.stateflowsLock = stateflowsLock;
        this.valueStorage = valueStorage;
        this.logger = logger;
        runtimeEngine = new RuntimeEngine(this);
    }

    public async Task<bool> ProcessAsync<TMessage>(IBeeMessageContext<TMessage, BeeStateflowsState> context)
    {
        if (context.Message is not ProcessBehaviorEventRequest request)
        {
            return false;
        }

        var status = EventStatus.Invalid;
        var responses = new Dictionary<object, EventHolder>();
        var validation = await request.EventHolder.ValidateAsync(validators);

        ResponseHolder.SetResponses(responses);
        try
        {
            await tenantExecutor.ExecuteByTenantAsync(request.TenantId, async () =>
            {
                try
                {
                    if (validation.IsValid)
                    {
                        var exceptions = new List<Exception>();
                        using var storageScope = BeeStorageBridge.EnterScope(
                            hydrateAsync: () => Task.FromResult(context.State.GetOrCreateContext(request.BehaviorId)),
                            dehydrateAsync: savedContext =>
                            {
                                context.State.Context = savedContext;
                                context.State.SyncSerialized();
                                return Task.CompletedTask;
                            }
                        );

                        status = await request.EventHolder.ProcessEventAsync(runtimeEngine, request.BehaviorId, exceptions, responses);
                        foreach (var exception in exceptions)
                        {
                            logger.LogError(exception, "Stateflows behavior {BehaviorId} execution failed.", request.BehaviorId);
                        }
                    }
                    else
                    {
                        status = EventStatus.Invalid;
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Unhandled Stateflows runtime exception in bee {BeeId}.", context.Id);
                    status = EventStatus.Failed;
                }
            });

            await request.RespondAsync(
                new ProcessBehaviorEventResponse(
                    status,
                    validation,
                    request.EventHolder.GetResponseHolder()
                )
            );
        }
        finally
        {
            ResponseHolder.ClearResponses();
        }

        return true;
    }

    private sealed class RuntimeEngine(StateflowsRuntimeBee runtime) : IStateflowsEngine
    {
        public async Task<EventStatus> ProcessEventAsync<TEvent>(
            BehaviorId id,
            EventHolder<TEvent> eventHolder,
            List<Exception> exceptions,
            Dictionary<object, EventHolder> responses
        )
        {
            if (!runtime.processorsByType.TryGetValue(id.Type, out var processor))
            {
                return EventStatus.Undelivered;
            }

            if (!runtime.interceptor.BeforeExecute(id, eventHolder))
            {
                return EventStatus.Undelivered;
            }

            if (eventHolder.BoxedPayload is Finalize { Mode: FinalizationMode.Immediate })
            {
                await runtime.valueStorage.SetAsync(id, ForceFinalizeKey, true);
                await processor.CancelProcessingAsync(id);
            }

            await using var lockHandle = await (
                id.Type is not BehaviorType.Activity and not BehaviorType.Entity and not BehaviorType.StateMachine
                    ? runtime.stateflowsLock.AquireNoLockAsync(id)
                    : runtime.stateflowsLock.AquireLockAsync(id)
            );

            try
            {
                ResponseHolder.SetResponses(responses);
                return await processor.ProcessEventAsync(id, eventHolder, exceptions);
            }
            finally
            {
                var responseHolder = eventHolder.GetResponseHolder();
                if (responseHolder != null)
                {
                    responseHolder.SenderId = id;
                    responseHolder.SentAt = DateTime.Now;
                }

                runtime.interceptor.AfterExecute(id, eventHolder);
            }
        }
    }
}
