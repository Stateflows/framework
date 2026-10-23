using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.StateMachines.Context.Interfaces;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class ObserverPipelines : StateflowsTestClass
{
    private readonly ConcurrentQueue<string> trace = new();
    private readonly ConcurrentQueue<(string Hook, object? Result)> results = new();
    private readonly ConcurrentQueue<bool> implicitInitializations = new();
    private readonly Exception original = new ApplicationException("model failure");
    private string? violation;
    private bool failOperation;
    private bool initializationAccepted = true;
    private bool guardAccepted = true;
    private int operations;

    public record Advance;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        var globals = new[] { new Recorder(this, "g1"), new Recorder(this, "g2") };
        var locals = new[] { new Recorder(this, "l1"), new Recorder(this, "l2") };
        builder.AddActions(actions =>
        {
            actions.AddAction("around", _ => ExecuteOperationAsync(), action =>
            {
                foreach (var observer in locals)
                    action.AddObserver(_ => observer);
            });
            foreach (var observer in globals)
                actions.AddObserver(_ => Task.FromResult<IActionObserver>(observer));
        });
        builder.AddStateMachines(machines =>
        {
            machines.AddStateMachine("around", machine =>
            {
                machine.AddDefaultInitializer(_ => Task.FromResult(initializationAccepted))
                    .AddInitialState("source", state => state
                        .AddOnEntry(_ => ExecuteOperationAsync())
                        .AddTransition<Advance>("target", transition => transition
                            .AddGuard(_ => Task.FromResult(guardAccepted))
                            .AddEffect(_ => Task.CompletedTask)))
                    .AddFinalState("target");
                foreach (var observer in locals)
                    machine.AddObserver((_, _) => observer);
            });
            machines.AddStateMachine("no-suitable", machine =>
            {
                machine.AddInitializer<Advance>(_ => Task.FromResult(true))
                    .AddInitialState("source");
                foreach (var observer in locals)
                    machine.AddObserver((_, _) => observer);
            });
            foreach (var observer in globals)
                machines.AddObserver((_, _) => observer);
        });
        builder.AddActivities(activities =>
        {
            activities.AddActivity("around", activity =>
            {
                activity.AddDefaultInitializer(_ => Task.FromResult(initializationAccepted))
                    .AddInitial(initial => initial.AddControlFlow("source"))
                    .AddAction("source", async context =>
                    {
                        await ExecuteOperationAsync();
                        context.Output(42);
                    }, node => node.AddFlow<int>("target", flow => flow
                        .AddGuard(_ => Task.FromResult(guardAccepted))
                        .AddTransformation<string>(_ => Task.FromResult("answer"))))
                    .AddAction("target", _ => Task.CompletedTask, node => node.AddControlFlow<FinalNode>())
                    .AddFinal();
                foreach (var observer in locals)
                    activity.AddObserver(_ => observer);
            });
            foreach (var observer in globals)
                activities.AddObserver(_ => observer);
        });
    }

    private async Task ExecuteOperationAsync()
    {
        await Task.Yield();
        operations++;
        if (failOperation)
            throw original;
    }

    private Task<(EventStatus Status, List<Exception> Exceptions)> ExecuteAsync(string type)
        => ExecuteAsync(type, new Initialize());

    private async Task<(EventStatus Status, List<Exception> Exceptions)> ExecuteAsync<TEvent>(string type, TEvent @event, string name = "around")
    {
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var processor = ServiceProvider.GetServices<IEventProcessor>()
            .Single(processor => processor.BehaviorTypes.Contains(type));
        var exceptions = new List<Exception>();
        var status = await processor.ProcessEventAsync(new BehaviorId(type, name, "x"),
            @event.ToEventHolder(), exceptions);
        return (status, exceptions);
    }

    private void AssertOrder(string hook)
    {
        var scopes = new[] { "g1", "g2", "l1", "l2" };
        var expected = scopes.Select(scope => $"{hook}:{scope}:before")
            .Concat(scopes.Reverse().Select(scope => $"{hook}:{scope}:after")).ToArray();
        CollectionAssert.AreEqual(expected, trace.Where(entry => entry.StartsWith(hook + ":")).ToArray());
    }

    [TestMethod]
    [DataRow(BehaviorType.Action, "ActionInitialize")]
    [DataRow(BehaviorType.Activity, "NodeExecute:source")]
    [DataRow(BehaviorType.StateMachine, "StateEntry:source")]
    public async Task GlobalsWrapLocals_InRegistrationOrder(string type, string hook)
    {
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreNotEqual(EventStatus.Failed, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(1, operations);
        AssertOrder(hook);
    }

    [TestMethod]
    public async Task FinalizationAndGuardHooks_ExposeResults_WithoutChangingExecution()
    {
        await ExecuteAsync(BehaviorType.Action);
        await ExecuteAsync(BehaviorType.Action, new Finalize());
        AssertOrder("ActionFinalize");
        await ExecuteAsync(BehaviorType.StateMachine);
        AssertOrder("StateMachineInitialize");
        var (status, exceptions) = await ExecuteAsync(BehaviorType.StateMachine, new Advance());
        Assert.AreNotEqual(EventStatus.Failed, status);
        Assert.AreEqual(0, exceptions.Count);
        AssertOrder("TransitionGuard");
        AssertOrder("TransitionEffect");
        AssertOrder("StateExit");
        AssertOrder("StateMachineFinalize");
        Assert.AreEqual(4, results.Count(result => result.Hook == "TransitionGuard" && Equals(result.Result, true)));
        Assert.AreEqual(4, results.Count(result => result.Hook == "StateMachineInitialize" && Equals(result.Result, true)));
    }

    [TestMethod]
    public async Task ActivityHooks_ExposeInitializationGuardTransformationAndActivationResults()
    {
        var (status, exceptions) = await ExecuteAsync(BehaviorType.Activity);
        Assert.AreNotEqual(EventStatus.Failed, status);
        Assert.AreEqual(0, exceptions.Count);
        foreach (var hook in new[] { "ActivityInitialize", "ActivityFinalize", "FlowGuard", "FlowTransform", "FlowActivate:source:target", "NodeActivate:source" })
            AssertOrder(hook);
        Assert.AreEqual(4, results.Count(result => result.Hook == "FlowTransform" && Equals(result.Result, "answer")));
        Assert.AreEqual(4, results.Count(result => result.Hook == "FlowGuard" && Equals(result.Result, true)));
        Assert.AreEqual(4, results.Count(result => result.Hook == "ActivityInitialize" && Equals(result.Result, true)));
        Assert.IsTrue(results.Where(result => result.Hook == "FlowActivate:source:target")
            .All(result => result.Result is IActivityAfterFlowContext { Activated: true, SourceTokenCount: 1, TargetTokenCount: 1 }));
    }

    [TestMethod]
    [DataRow(BehaviorType.Activity, "ActivityInitialize")]
    [DataRow(BehaviorType.StateMachine, "StateMachineInitialize")]
    public async Task RejectedInitialization_IsObservedAsFalse_AndDoesNotExecuteTheModel(string type, string hook)
    {
        initializationAccepted = false;
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreNotEqual(EventStatus.Failed, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(0, operations);
        AssertOrder(hook);
        Assert.AreEqual(4, results.Count(result => result.Hook == hook && Equals(result.Result, false)));
    }

    [TestMethod]
    public async Task MissingInitializer_IsObservedAsFalse()
    {
        var (status, exceptions) = await ExecuteAsync(BehaviorType.StateMachine, new Initialize(), "no-suitable");
        Assert.AreNotEqual(EventStatus.Failed, status);
        Assert.AreEqual(0, exceptions.Count);
        AssertOrder("StateMachineInitialize");
        Assert.AreEqual(4, results.Count(result => result.Hook == "StateMachineInitialize" && Equals(result.Result, false)));
    }

    [TestMethod]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    public async Task ImplicitInitializationFlag_IsAvailableBeforeContinuation(string type)
    {
        var (status, exceptions) = await ExecuteAsync(type, new Advance());
        Assert.AreNotEqual(EventStatus.Failed, status);
        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(4, implicitInitializations.Count);
        Assert.IsTrue(implicitInitializations.All(implicitInitialization => implicitInitialization));
    }

    [TestMethod]
    [DataRow(BehaviorType.Activity, "FlowGuard")]
    [DataRow(BehaviorType.StateMachine, "TransitionGuard")]
    public async Task RejectedGuards_AreObservedAsFalse_AndDoNotRunLaterOperations(string type, string hook)
    {
        guardAccepted = false;
        await ExecuteAsync(type);
        if (type == BehaviorType.StateMachine)
            await ExecuteAsync(type, new Advance());
        AssertOrder(hook);
        Assert.AreEqual(4, results.Count(result => result.Hook == hook && Equals(result.Result, false)));
        Assert.IsFalse(results.Any(result => result.Hook == "FlowTransform"));
        Assert.IsFalse(trace.Any(entry => entry.StartsWith("TransitionEffect:")));
        if (type == BehaviorType.Activity)
        {
            Assert.AreEqual(4, results.Count(result => result.Hook == "FlowActivate:source:target"));
            Assert.IsTrue(results.Where(result => result.Hook == "FlowActivate:source:target")
                .All(result => result.Result is IActivityAfterFlowContext { Activated: false, TargetTokenCount: 0 }));
        }
    }

    [TestMethod]
    [DataRow(BehaviorType.Action, "missing")]
    [DataRow(BehaviorType.Action, "duplicate")]
    [DataRow(BehaviorType.Activity, "missing")]
    [DataRow(BehaviorType.Activity, "duplicate")]
    [DataRow(BehaviorType.StateMachine, "missing")]
    [DataRow(BehaviorType.StateMachine, "duplicate")]
    public async Task InvalidContinuations_FailTheEvent(string type, string misuse)
    {
        violation = misuse;
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreEqual(EventStatus.Failed, status);
        Assert.IsTrue(exceptions.Any(exception => exception is InvalidOperationException));
        Assert.IsTrue(operations <= 1);
    }

    [TestMethod]
    [DataRow(BehaviorType.Action, "throw-before")]
    [DataRow(BehaviorType.Action, "throw-after")]
    [DataRow(BehaviorType.Activity, "throw-before")]
    [DataRow(BehaviorType.Activity, "throw-after")]
    [DataRow(BehaviorType.StateMachine, "throw-before")]
    [DataRow(BehaviorType.StateMachine, "throw-after")]
    public async Task ObserverFailures_FailExecutionAndRollBackInitialization(string type, string failure)
    {
        violation = failure;
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreEqual(EventStatus.Failed, status);
        Assert.IsTrue(exceptions.Any(exception => ReferenceEquals(exception, original)));
        ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
            ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
        var stored = await ServiceProvider.GetRequiredService<IStateflowsStorage>()
            .HydrateAsync(new BehaviorId(type, "around", "x"));
        Assert.AreNotEqual(BehaviorStatus.Initialized, stored.Status);
    }

    [TestMethod]
    [DataRow(BehaviorType.Activity)]
    [DataRow(BehaviorType.StateMachine)]
    public async Task SwallowedModelFailures_RemainFailed_AndFinallyHooksRun(string type)
    {
        violation = "swallow";
        failOperation = true;
        var (status, exceptions) = await ExecuteAsync(type);
        Assert.AreEqual(EventStatus.Failed, status);
        Assert.IsTrue(exceptions.Any(exception => ReferenceEquals(exception, original)));
        Assert.IsTrue(trace.Any(entry => entry.EndsWith(":g1:after")));
    }

    [TestMethod]
    public async Task UnfinishedContinuation_IsDrainedAndRejected()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var operationFinished = false;
        var execution = ObserverPipeline.RunAsync(new[] { new object() }, async () =>
        {
            entered.SetResult();
            await release.Task;
            operationFinished = true;
            return 42;
        }, (_, next) =>
        {
            _ = next();
            return Task.CompletedTask;
        });
        await entered.Task;
        Assert.IsFalse(execution.IsCompleted);
        release.SetResult();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await execution);
        Assert.IsTrue(operationFinished);
    }

    private sealed class Recorder(ObserverPipelines owner, string scope)
        : StateMachineObserver, IActionObserver, IActivityObserver
    {
        private async Task ObserveAsync(string hook, Func<Task> next)
        {
            owner.trace.Enqueue($"{hook}:{scope}:before");
            await Task.Yield();
            try
            {
                var selected = hook is "ActionInitialize" or "StateEntry:source" or "NodeExecute:source";
                if (scope == "g1" && selected && owner.violation == "missing")
                    return;
                if (scope == "g1" && selected && owner.violation == "throw-before")
                    throw owner.original;
                try
                {
                    await next();
                }
                catch (ApplicationException) when (scope == "g1" && selected && owner.violation == "swallow")
                {
                }
                if (scope == "g1" && selected && owner.violation == "duplicate")
                    await next();
                if (scope == "g1" && selected && owner.violation == "throw-after")
                    throw owner.original;
            }
            finally
            {
                owner.trace.Enqueue($"{hook}:{scope}:after");
            }
        }

        private Task ObserveResultAsync<TResult>(string hook, Func<Task<TResult>> next)
            => ObserveAsync(hook, async () => owner.results.Enqueue((hook, await next())));

        public Task OnActionInitializeAsync(IActionDelegateContext context, Func<Task> next)
            => ObserveAsync("ActionInitialize", next);
        public Task OnActionFinalizeAsync(IActionDelegateContext context, Func<Task> next)
            => ObserveAsync("ActionFinalize", next);
        public override Task OnStateMachineInitializeAsync(IStateMachineInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            owner.implicitInitializations.Enqueue(implicitInitialization);
            return ObserveResultAsync("StateMachineInitialize", next);
        }
        public override Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
            => ObserveAsync("StateMachineFinalize", next);
        public override Task OnStateEntryAsync(IStateActionContext context, Func<Task> next)
            => ObserveAsync("StateEntry:" + context.State.Name, next);
        public override Task OnStateExitAsync(IStateActionContext context, Func<Task> next)
            => ObserveAsync("StateExit", next);
        public override Task OnTransitionGuardAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task<bool>> next)
            => ObserveResultAsync("TransitionGuard", next);
        public override Task OnTransitionEffectAsync<TEvent>(ITransitionContext<TEvent> context, Func<Task> next)
            => ObserveAsync("TransitionEffect", next);
        public Task OnActivityInitializeAsync(IActivityInitializationContext context, bool implicitInitialization, Func<Task<bool>> next)
        {
            owner.implicitInitializations.Enqueue(implicitInitialization);
            return ObserveResultAsync("ActivityInitialize", next);
        }
        public Task OnActivityFinalizeAsync(IActivityFinalizationContext context, Func<Task> next)
            => ObserveAsync("ActivityFinalize", next);
        public Task OnNodeInitializeAsync(IActivityNodeContext context, Func<Task> next)
            => ObserveAsync("NodeInitialize:" + context.Node.Name, next);
        public Task OnNodeFinalizeAsync(IActivityNodeContext context, Func<Task> next)
            => ObserveAsync("NodeFinalize:" + context.Node.Name, next);
        public Task OnNodeActivateAsync(IActivityNodeContext context, bool activated, Func<Task> next)
            => ObserveAsync("NodeActivate:" + context.Node.Name, next);
        public Task OnNodeExecuteAsync(IActivityNodeContext context, Func<Task> next)
            => ObserveAsync("NodeExecute:" + context.Node.Name, next);
        public Task OnFlowActivateAsync(IActivityBeforeFlowContext context, Func<Task<IActivityAfterFlowContext>> next)
            => ObserveResultAsync($"FlowActivate:{context.SourceNode.Name}:{context.TargetNode.Name}", next);
        public Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> context, Func<Task<bool>> next)
            => ObserveResultAsync("FlowGuard", next);
        public Task OnFlowTransformAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Func<Task<TTransformedToken>> next)
            => ObserveResultAsync("FlowTransform", next);
    }
}
