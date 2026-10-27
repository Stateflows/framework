using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Context.Interfaces;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.StateMachines;
using ActionEventContext = Stateflows.Actions.Context.Interfaces;
using ActivityEventContext = Stateflows.Activities.Context.Interfaces;
using MachineEventContext = Stateflows.StateMachines.Context.Interfaces;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class InterceptorPipelines : StateflowsTestClass
{
    private readonly ConcurrentQueue<string> trace = new();
    private readonly Exception original = new ApplicationException("interceptor pipeline failure");
    private string? mode;
    private string target = "g1";
    private int operations;
    private int responses;

    public record Advance;
    public record Query : IRequest<string>;

    public interface ITemplate
    {
        [Field]
        int Value { get; set; }
    }

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddInterceptor(_ => new Recorder(this, "c1"));
        builder.AddInterceptor(_ => new Recorder(this, "c2"));
        var globals = new[] { new Recorder(this, "g1"), new Recorder(this, "g2") };
        var locals = new[] { new Recorder(this, "l1"), new Recorder(this, "l2") };
        builder.AddActions(actions =>
        {
            actions.AddAction("intercepted", _ => OperateAsync(), action =>
            {
                foreach (var interceptor in locals)
                    action.AddInterceptor(_ => interceptor);
            });
            foreach (var interceptor in globals)
                actions.AddInterceptor(_ => interceptor);
        });
        builder.AddActivities(activities =>
        {
            activities.AddActivity("intercepted", activity =>
            {
                activity.AddInitial(initial => initial.AddControlFlow("work"))
                    .AddAction("work", _ => OperateAsync(), node => node.AddControlFlow<FinalNode>())
                    .AddFinal();
                foreach (var interceptor in locals)
                    activity.AddInterceptor(_ => interceptor);
            });
            foreach (var interceptor in globals)
                activities.AddInterceptor(_ => interceptor);
        });
        builder.AddStateMachines(machines =>
        {
            machines.AddStateMachine("intercepted", machine =>
            {
                machine.AddInitialState("ready", state => state.AddOnEntry(_ => OperateAsync()));
                foreach (var interceptor in locals)
                    machine.AddInterceptor((_, _) => interceptor);
            });
            foreach (var interceptor in globals)
                machines.AddInterceptor((_, _) => interceptor);
        });
        builder.AddEntities(entities =>
        {
            foreach (var interceptor in globals)
                entities.AddInterceptor((_, _) => interceptor);
            entities.AddEntity<ITemplate>("intercepted", entity =>
            {
                entity.AddDefaultInitializer(context =>
                {
                    context.Entity.Value = 42;
                    operations++;
                    trace.Enqueue("operation");
                    if (mode == "model-failure")
                        throw original;
                });
                foreach (var interceptor in locals)
                    entity.AddInterceptor((_, _) => interceptor);
            });
        });
    }

    private async Task OperateAsync()
    {
        operations++;
        trace.Enqueue("operation");
        await Task.Delay(mode == "unfinished" ? 50 : 1);
        if (mode == "model-failure")
            throw original;
    }

    private Task<(EventStatus Status, List<Exception> Exceptions)> ExecuteAsync(string type)
        => ExecuteAsync(type, new Advance());

    private async Task<(EventStatus Status, List<Exception> Exceptions)> ExecuteAsync<TEvent>(string type, TEvent @event)
    {
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var processor = ServiceProvider.GetServices<IEventProcessor>()
            .Single(processor => processor.BehaviorTypes.Contains(type));
        var exceptions = new List<Exception>();
        var status = await processor.ProcessEventAsync(new BehaviorId(type, "intercepted", "x"),
            @event.ToEventHolder(), exceptions);
        return (status, exceptions);
    }

    private async Task AssertNotInitializedAsync(string type)
    {
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var stored = await ServiceProvider.GetRequiredService<IStateflowsStorage>()
            .HydrateAsync(new BehaviorId(type, "intercepted", "x"));
        Assert.IsTrue(stored.Status is BehaviorStatus.Unknown or BehaviorStatus.NotInitialized);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Entity)]
    public async Task CommonThenGlobalThenLocal_WrapImplicitInitialization(string type)
    {
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreNotEqual(EventStatus.Failed, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(1, operations);
        var names = new[] { "c1", "c2", "g1", "g2", "l1", "l2" };
        CollectionAssert.AreEqual(names.Select(name => $"{name}:before")
            .Append("operation").Concat(names.Reverse().Select(name => $"{name}:after")).ToArray(), trace.ToArray());
    }

    [TestMethod]
    [DataRow(BehaviorType.Action, "c1")]
    [DataRow(BehaviorType.Action, "g1")]
    [DataRow(BehaviorType.Action, "l1")]
    [DataRow(BehaviorType.Activity, "c1")]
    [DataRow(BehaviorType.Activity, "g1")]
    [DataRow(BehaviorType.Activity, "l1")]
    [DataRow(BehaviorType.StateMachine, "c1")]
    [DataRow(BehaviorType.StateMachine, "g1")]
    [DataRow(BehaviorType.StateMachine, "l1")]
    [DataRow(BehaviorType.Entity, "c1")]
    [DataRow(BehaviorType.Entity, "g1")]
    [DataRow(BehaviorType.Entity, "l1")]
    [DataRow(BehaviorType.Action, "g1", true)]
    [DataRow(BehaviorType.Activity, "g1", true)]
    [DataRow(BehaviorType.StateMachine, "g1", true)]
    [DataRow(BehaviorType.Entity, "g1", true)]
    public async Task ShortCircuit_DoesNotInitializeOrExecute(string type, string scope, bool explicitInitialization = false)
    {
        mode = "block";
        target = scope;
        var (status, exceptions) = explicitInitialization
            ? await ExecuteAsync(type, new Initialize())
            : await ExecuteAsync(type);
        Assert.AreEqual(EventStatus.Rejected, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(0, operations);
        await AssertNotInitializedAsync(type);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Entity)]
    public async Task ReplacementStatus_IsReturnedAfterProcessing(string type)
    {
        mode = "replace";
        target = "l1";
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreEqual(EventStatus.Invalid, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(1, operations);
    }

    [TestMethod]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    public async Task UnscheduledTimeEvent_RemainsUndeliveredWithoutInitializing(string type)
    {
        var (status, exceptions) = await ExecuteAsync(type, new TimeEvent());
        Assert.AreEqual(EventStatus.Undelivered, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(0, operations);
        await AssertNotInitializedAsync(type);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action, "double")]
    [DataRow(BehaviorType.Activity, "double")]
    [DataRow(BehaviorType.StateMachine, "double")]
    [DataRow(BehaviorType.Entity, "double")]
    [DataRow(BehaviorType.Action, "unfinished")]
    [DataRow(BehaviorType.Activity, "unfinished")]
    [DataRow(BehaviorType.StateMachine, "unfinished")]
    [DataRow(BehaviorType.Entity, "unfinished")]
    [DataRow(BehaviorType.Action, "before-failure")]
    [DataRow(BehaviorType.Activity, "before-failure")]
    [DataRow(BehaviorType.StateMachine, "before-failure")]
    [DataRow(BehaviorType.Entity, "before-failure")]
    [DataRow(BehaviorType.Action, "after-failure")]
    [DataRow(BehaviorType.Activity, "after-failure")]
    [DataRow(BehaviorType.StateMachine, "after-failure")]
    [DataRow(BehaviorType.Entity, "after-failure")]
    public async Task InvalidOrFailingInterceptor_FailsAndRollsBack(string type, string failure)
    {
        mode = failure;
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreEqual(EventStatus.Failed, status);
        Assert.AreEqual(failure == "before-failure" ? 0 : 1, operations);
        Assert.IsTrue(exceptions.Count > 0);
        if (failure.EndsWith("-failure"))
            Assert.IsTrue(exceptions.Any(exception => ReferenceEquals(original, exception)));
        else
            Assert.IsTrue(exceptions.Any(exception => exception is InvalidOperationException));
        await AssertNotInitializedAsync(type);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Entity)]
    public async Task CatchingRecordedModelFailure_DoesNotRecoverExecution(string type)
    {
        mode = "model-failure";
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreEqual(EventStatus.Failed, status);
        Assert.IsTrue(trace.Contains("handled"));
        Assert.IsTrue(exceptions.Any(exception => ReferenceEquals(original, exception)));
        await AssertNotInitializedAsync(type);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action)]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    [DataRow(BehaviorType.Entity)]
    public async Task InterceptorMayHandleUnrecordedContinuationFailure(string type)
    {
        mode = "handled-interceptor";
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreEqual(EventStatus.Rejected, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(0, operations);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action, "c1")]
    [DataRow(BehaviorType.Activity, "c1")]
    [DataRow(BehaviorType.StateMachine, "c1")]
    [DataRow(BehaviorType.Entity, "c1")]
    [DataRow(BehaviorType.Action, "g1")]
    [DataRow(BehaviorType.Activity, "g1")]
    [DataRow(BehaviorType.StateMachine, "g1")]
    [DataRow(BehaviorType.Entity, "g1")]
    public async Task ShortCircuitResponse_IsReportedOnce(string type, string scope)
    {
        mode = "respond";
        target = scope;
        var locator = ServiceProvider.GetRequiredService<IBehaviorLocator>();
        Assert.IsTrue(locator.TryLocateBehavior(new BehaviorId(type, "intercepted", "x"), out var behavior));
        var result = await behavior.RequestAsync(new Query());
        Assert.AreEqual(EventStatus.Consumed, result.Status);
        Assert.AreEqual("intercepted", result.Response);
        Assert.AreEqual(0, operations);
        Assert.AreEqual(1, responses);
    }

    private sealed class Recorder(InterceptorPipelines owner, string name) : BehaviorInterceptor,
        IActionInterceptor, IActivityInterceptor, IStateMachineInterceptor, IEntityInterceptor
    {
        public override Task<EventStatus> ProcessEventAsync<TEvent>(IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => InterceptAsync(context.Event, next);

        public Task<EventStatus> ProcessEventAsync<TEvent>(ActionEventContext.IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => InterceptAsync(context.Event, next);

        public Task<EventStatus> ProcessEventAsync<TEvent>(ActivityEventContext.IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => InterceptAsync(context.Event, next);

        public Task<EventStatus> ProcessEventAsync<TEvent>(MachineEventContext.IEventContext<TEvent> context, Func<Task<EventStatus>> next)
            => InterceptAsync(context.Event, next);

        private async Task<EventStatus> InterceptAsync<TEvent>(TEvent @event, Func<Task<EventStatus>> next)
        {
            owner.trace.Enqueue($"{name}:before");
            await Task.Yield();
            if (name == owner.target)
            {
                if (owner.mode == "block") return EventStatus.Rejected;
                if (owner.mode == "respond" && @event is Query query)
                {
                    query.Respond("intercepted");
                    return EventStatus.Consumed;
                }
                if (owner.mode == "before-failure") throw owner.original;
                if (owner.mode == "unfinished")
                {
                    _ = next();
                    return EventStatus.Consumed;
                }
            }
            if (name == "l1" && owner.mode == "handled-interceptor") throw owner.original;
            EventStatus status;
            try
            {
                status = await next();
            }
            catch (Exception exception) when (ReferenceEquals(exception, owner.original) && name == owner.target &&
                owner.mode is "model-failure" or "handled-interceptor")
            {
                owner.trace.Enqueue("handled");
                return EventStatus.Rejected;
            }
            if (name == owner.target)
            {
                if (owner.mode == "double")
                {
                    try { await next(); }
                    catch (InvalidOperationException) { }
                }
                if (owner.mode == "after-failure") throw owner.original;
                if (owner.mode == "replace") status = EventStatus.Invalid;
            }
            owner.trace.Enqueue($"{name}:after");
            return status;
        }

        public override Task RequestRespondedAsync<TRequest, TResponse>(
            IBehaviorActionContext context, TRequest request, TResponse response)
        {
            if (name == "c1")
            {
                Assert.AreEqual<object>("intercepted", response);
                owner.responses++;
            }
            return Task.CompletedTask;
        }
    }
}
