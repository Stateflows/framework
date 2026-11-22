using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

[TestClass]
public sealed class OperationObservation : StateflowsTestClass
{
    public sealed class Start;
    public sealed class Declined;
    public sealed class Accept : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token) => Task.FromResult(true);
    }
    public sealed class Reject : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token) => Task.FromResult(false);
    }
    public sealed class Never : IFlowGuard<int>
    {
        public Task<bool> GuardAsync(int token) => throw new AssertFailedException("A short-circuited guard was invoked.");
    }
    public sealed class ToText : IFlowTransformation<int, string>
    {
        public Task<string> TransformAsync(int token) => Task.FromResult(token.ToString());
    }

    private sealed record Definition(string Owner, int Order, ActivityOperationKind? Operation = null,
        ActivityGuardKind? Guard = null, string? Parent = null);

    private sealed class DefinitionRecorder : ActivityVisitor
    {
        public readonly Dictionary<string, Definition> Operations = [];
        public readonly Dictionary<string, string> Nodes = [];
        public override Task NodeAddedAsync(string id, string name, int version, string nodeName, NodeType nodeType, string? parentNodeId)
        {
            Nodes.Add(id, nodeName);
            return Task.CompletedTask;
        }
        public override Task NodeOperationAddedAsync(string id, string name, int version, string nodeId, ActivityOperationKind kind, int order)
        {
            Operations.Add(id, new(nodeId, order, kind));
            return Task.CompletedTask;
        }
        public override Task DefaultInitializerAddedAsync(string id, string name, int version, int order)
        {
            Operations.Add(id, new("Graph", order, ActivityOperationKind.DefaultInitialize));
            return Task.CompletedTask;
        }
        public override Task InitializerAddedAsync<TEvent>(string id, string name, int version, IReadOnlyList<string> eventNames, int order)
        {
            Operations.Add(id, new("Graph", order, ActivityOperationKind.Initialize));
            return Task.CompletedTask;
        }
        public override Task FinalizerAddedAsync(string id, string name, int version, int order)
        {
            Operations.Add(id, new("Graph", order, ActivityOperationKind.Finalize));
            return Task.CompletedTask;
        }
        public override Task FlowGuardExpressionAddedAsync(string id, string name, int version, string flowId,
            string? parentGuardId, ActivityGuardKind kind, int order)
        {
            Operations.Add(id, new(flowId, order, Guard: kind, Parent: parentGuardId));
            return Task.CompletedTask;
        }
        public override Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string id, string name, int version, string flowId, int order)
        {
            Operations.Add(id, new(flowId, order));
            return Task.CompletedTask;
        }
    }

    private sealed record Stage(string Id, string FlowId, int Order, object? Token, Guid TokenId, Type Input, Type Output);

    private sealed class Recorder : ActivityObserver
    {
        public readonly ConcurrentQueue<IActivityOperationObservationContext> Entered = new();
        public readonly ConcurrentDictionary<string, object?> Results = new();
        public readonly ConcurrentQueue<Stage> Transformations = new();
        public readonly ConcurrentQueue<string> Aggregate = new();
        public bool DuplicateTransformation;
        public Exception? SwallowedFailure;
        public Exception? FailureToSwallow;
        private readonly AsyncLocal<int> guardDepth = new();
        private readonly AsyncLocal<int> initializationDepth = new();

        public override async Task OnActionAsync(IActivityActionObservationContext context, Func<Task> next)
        {
            Entered.Enqueue(context);
            try { await next(); }
            catch (Exception exception) when (ReferenceEquals(exception, FailureToSwallow))
            {
                SwallowedFailure = exception;
            }
        }
        public override async Task OnActivityInitializeAsync(
            IActivityInitializationContext initializationContext, bool implicitInitialization, Func<Task<bool>> next)
        {
            Aggregate.Enqueue("initialize");
            initializationDepth.Value++;
            try { await next(); }
            finally { initializationDepth.Value--; }
        }
        public override async Task OnPredicateAsync(IActivityPredicateObservationContext context, Func<Task<bool>> next)
        {
            if (context.Operation.HasValue)
            {
                Assert.IsTrue(initializationDepth.Value > 0, "Initializer predicates must stay inside whole-operation initialization.");
                Assert.IsNull(context.TokenType);
                Assert.IsNull(context.Token);
                Assert.IsNull(context.TokenId);
                Assert.IsNull(context.ParentGuardId);
                Assert.AreEqual(ActivityGuardKind.Leaf, context.Kind);
            }
            else
            {
                Assert.IsTrue(guardDepth.Value > 0, "Flow predicates must stay inside their enclosing flow-guard evaluation.");
                Assert.IsNotNull(context.TokenType);
                Assert.IsTrue(context.TokenId.HasValue);
            }
            Entered.Enqueue(context);
            Results[context.Identifier] = await next();
        }
        public override async Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> context, Func<Task<bool>> next)
        {
            Aggregate.Enqueue("guard");
            guardDepth.Value++;
            try { await next(); }
            finally { guardDepth.Value--; }
        }
        public override async Task OnFlowTransformAsync<TToken, TTransformedToken>(
            ITransformationContext<TToken> context, Func<Task<TTransformedToken>> next)
        {
            Aggregate.Enqueue("transform");
            Transformations.Enqueue(new(context.TransformationId, context.Identifier, context.TransformationOrder,
                context.Token, context.TokenId, context.InputTokenType, context.OutputTokenType));
            Results[context.TransformationId] = await next();
            if (DuplicateTransformation && context.OutputTokenType == typeof(int))
                await next();
        }
        public override Task OnNodeExecuteAsync(IActivityNodeContext context, Func<Task> next)
        {
            Aggregate.Enqueue("execute");
            return next();
        }
    }

    private readonly Recorder observer = new();
    private readonly ConcurrentQueue<string> invoked = new();
    private int transformations;
    private int declinedActions;
    private readonly Exception originalFailure = new ApplicationException("registered action failed");

    [TestInitialize]
    public override void Initialize() => base.Initialize();
    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddActivities(activities => activities
            .AddActivity("observed-pipeline", activity => activity
                .AddDefaultInitializer(_ => Task.FromResult(true))
                .AddInputPin<int>(pin => pin.AddFlow("result", flow => flow
                    .AddGuard(_ => Task.FromResult(true), _ => Task.FromResult(true))
                    .AddOrGuards<Accept, Never>()
                    .AddTransformation<string, ToText>()
                    .AddGuard(context => Task.FromResult(context.Token == "12"))
                    .AddTransformation<int>(context =>
                    {
                        transformations++;
                        return Task.FromResult(int.Parse(context.Token) + 1);
                    })))
                .AddOutputPin<int>("result", pin => pin.SetTimeToLive(300))
                .AddObserver(_ => observer))
            .AddActivity("observed-rejected-pipeline", activity => activity
                .AddInputPin<int>(pin => pin.AddFlow("result", flow => flow
                    .AddAndGuards<Reject, Never>()
                    .AddGuard(_ => throw new AssertFailedException("A stage after rejection was invoked."))
                    .AddTransformation<int>(_ =>
                    {
                        transformations++;
                        return Task.FromResult(1);
                    })))
                .AddOutputPin<int>("result", pin => pin.SetTimeToLive(300))
                .AddObserver(_ => observer))
            .AddActivity("observed-lifecycle", activity => activity
                .AddInitializer<Start>(_ => { invoked.Enqueue("event-0"); return Task.FromResult(true); })
                .AddInitializer<Start>(_ => { invoked.Enqueue("event-1"); return Task.FromResult(true); })
                .AddFinalizer(_ => { invoked.Enqueue("activity-finalize-0"); return Task.CompletedTask; })
                .AddFinalizer(_ => { invoked.Enqueue("activity-finalize-1"); return Task.CompletedTask; })
                .AddInitial(initial => initial.AddControlFlow("work"))
                .AddStructuredActivity("work", work => work
                    .AddOnInitialize(_ => { invoked.Enqueue("initialize-0"); return Task.CompletedTask; })
                    .AddOnInitialize(_ => { invoked.Enqueue("initialize-1"); return Task.CompletedTask; })
                    .AddOnFinalize(_ => { invoked.Enqueue("finalize-0"); return Task.CompletedTask; })
                    .AddOnFinalize(_ => { invoked.Enqueue("finalize-1"); return Task.CompletedTask; })
                    .AddInitial(initial => initial.AddControlFlow("execute"))
                    .AddAction("execute", _ => { invoked.Enqueue("execute"); return Task.CompletedTask; },
                        action => action.AddControlFlow("nested-final"))
                    .AddFinal("nested-final")
                    .AddControlFlow<FinalNode>())
                .AddFinal()
                .AddObserver(_ => observer))
            .AddActivity("observed-declined-initializer", activity => activity
                .AddInitializer<Declined>(_ => { invoked.Enqueue("declined-0"); return Task.FromResult(false); })
                .AddInitializer<Declined>(_ => { invoked.Enqueue("declined-1"); return Task.FromResult(true); })
                .AddInitial(initial => initial.AddControlFlow("execute"))
                .AddAction("execute", _ => { declinedActions++; return Task.CompletedTask; })
                .AddObserver(_ => observer))
            .AddActivity("observed-declined-default-initializer", activity => activity
                .AddDefaultInitializer(_ => { invoked.Enqueue("default-0"); return Task.FromResult(false); })
                .AddInitial(initial => initial.AddControlFlow("execute"))
                .AddAction("execute", _ => { declinedActions++; return Task.CompletedTask; })
                .AddObserver(_ => observer))
            .AddActivity("observed-failed-action", activity => activity
                .AddInitial(initial => initial.AddControlFlow("execute"))
                .AddAction("execute", _ => throw originalFailure)
                .AddObserver(_ => observer)));

    private IActivityBehavior Locate(string name)
    {
        Assert.IsTrue(ActivityLocator.TryLocateActivity(new ActivityId(name, Guid.NewGuid().ToString("N")), out var activity));
        return activity;
    }

    private async Task<DefinitionRecorder> Definitions(string name)
    {
        var visitor = new DefinitionRecorder();
        await ((ActivitiesRegister)ServiceProvider.GetRequiredService<IActivitiesRegister>()).VisitActivityAsync(name, 1, visitor);
        return visitor;
    }

    private void AssertIdentity(DefinitionRecorder definitions)
    {
        foreach (var stage in observer.Transformations)
        {
            Assert.IsTrue(definitions.Operations.TryGetValue(stage.Id, out var definition), stage.Id);
            Assert.AreEqual(definition!.Owner, stage.FlowId);
            Assert.AreEqual(definition.Order, stage.Order);
        }
        foreach (var context in observer.Entered)
        {
            Assert.IsTrue(definitions.Operations.TryGetValue(context.Identifier, out var definition), context.Identifier);
            Assert.AreEqual(definition!.Owner, context.OwnerId);
            Assert.AreEqual(definition.Order, context.Order);
            if (context is IActivityActionObservationContext action)
                Assert.AreEqual(definition.Operation, action.Operation);
            if (context is IActivityPredicateObservationContext predicate)
            {
                Assert.AreEqual(definition.Operation, predicate.Operation);
                Assert.AreEqual(definition.Parent, predicate.ParentGuardId);
                if (!predicate.Operation.HasValue)
                    Assert.AreEqual(definition.Guard, predicate.Kind);
            }
        }
        Assert.AreEqual(observer.Entered.Count, observer.Entered.Select(context => context.Identifier).Distinct().Count(),
            "Each actual registration is invoked once for this single-token execution.");
    }

    [TestMethod]
    public void PredicateContractHasOneIndividualBooleanCallback()
    {
        Assert.IsNull(typeof(IActivityObserver).GetMethod("OnInitializerAsync"));
        Assert.IsNull(typeof(IActivityObserver).GetMethod("OnGuardAsync"));
        var predicate = typeof(IActivityObserver).GetMethod(nameof(IActivityObserver.OnPredicateAsync));
        Assert.IsNotNull(predicate);
        Assert.AreEqual(typeof(IActivityPredicateObservationContext), predicate.GetParameters()[0].ParameterType);
        Assert.IsNotNull(typeof(IActivityObserver).GetMethod(nameof(IActivityObserver.OnActivityInitializeAsync)));
    }

    [TestMethod]
    public async Task ActualPipelineStagesShareDefinitionIdsAndPreservePayloadsAndShortCircuiting()
    {
        var definitions = await Definitions("observed-pipeline");
        var activity = Locate("observed-pipeline");
        var input = new TokensInput().Add(12);
        Assert.AreEqual(EventStatus.Consumed, (await activity.SendAsync(input)).Status);
        CollectionAssert.AreEqual(new[] { 13 }, (await activity.GetOutputAsync<int>(DateTime.MinValue)).ToArray());
        AssertIdentity(definitions);

        var guards = observer.Entered.OfType<IActivityPredicateObservationContext>().Where(predicate => !predicate.Operation.HasValue).ToArray();
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 0, 4 }, guards.Select(context => context.Order).ToArray());
        Assert.AreEqual(ActivityGuardKind.Or, guards[2].Kind);
        Assert.AreEqual(guards[2].Identifier, guards[3].ParentGuardId);
        Assert.AreEqual(4, guards.Count(context => context.TokenId == input.Tokens.Single().Id));
        Assert.IsFalse(observer.Entered.Any(context => context.Identifier == $"{guards[2].Identifier}/1"));
        Assert.IsTrue(guards.All(context => Equals(true, observer.Results[context.Identifier])));
        var stages = observer.Transformations.ToArray();
        CollectionAssert.AreEqual(new[] { typeof(int), typeof(string) }, stages.Select(stage => stage.Input).ToArray());
        CollectionAssert.AreEqual(new[] { typeof(string), typeof(int) }, stages.Select(stage => stage.Output).ToArray());
        Assert.AreEqual(12, stages[0].Token);
        Assert.AreEqual("12", stages[1].Token);
        Assert.AreEqual(input.Tokens.Single().Id, stages[0].TokenId);
        Assert.AreNotEqual(stages[0].TokenId, stages[1].TokenId);
        Assert.AreEqual(1, transformations);
        Assert.AreEqual(4, observer.Aggregate.Count(hook => hook == "guard"));
        Assert.AreEqual(2, observer.Aggregate.Count(hook => hook == "transform"));
        Assert.IsTrue(observer.Entered.OfType<IActivityPredicateObservationContext>()
            .Any(context => context.Operation == ActivityOperationKind.DefaultInitialize));
        var deliveries = observer.Entered.Where(context => context.Event is TokensInput).ToArray();
        Assert.IsTrue(deliveries.Length > 0);
        Assert.IsTrue(deliveries.All(context => context.EventId.HasValue && context.EventId != Guid.Empty));
        Assert.AreEqual(1, deliveries.Select(context => context.EventId).Distinct().Count());
        Assert.IsTrue(deliveries.All(context => context.EventName == typeof(TokensInput).GetEventName()));
    }

    [TestMethod]
    public async Task RejectedAndGroupDoesNotReportSkippedLeavesOrLaterPipelineStages()
    {
        var definitions = await Definitions("observed-rejected-pipeline");
        var activity = Locate("observed-rejected-pipeline");
        await activity.SendAsync(new TokensInput().Add(12));
        AssertIdentity(definitions);
        var guards = observer.Entered.OfType<IActivityPredicateObservationContext>().Where(predicate => !predicate.Operation.HasValue).ToArray();
        Assert.AreEqual(2, guards.Length);
        Assert.AreEqual(ActivityGuardKind.And, guards[0].Kind);
        Assert.AreEqual(guards[0].Identifier, guards[1].ParentGuardId);
        Assert.IsTrue(guards.All(context => Equals(false, observer.Results[context.Identifier])));
        Assert.AreEqual(0, observer.Transformations.Count);
        Assert.AreEqual(0, transformations);
        Assert.AreEqual(1, observer.Aggregate.Count(hook => hook == "guard"));
        Assert.AreEqual(0, (await activity.GetOutputAsync<int>(DateTime.MinValue)).Count());
    }

    [TestMethod]
    public async Task NodeAndActivityLifecycleRegistrationsUseFinalizedIdsAndAuthoredOrder()
    {
        var definitions = await Definitions("observed-lifecycle");
        await Locate("observed-lifecycle").SendAsync(new Start());
        AssertIdentity(definitions);
        CollectionAssert.AreEqual(new[] { "event-0", "event-1", "initialize-0", "initialize-1", "execute",
            "finalize-0", "finalize-1", "activity-finalize-0", "activity-finalize-1" }, invoked.ToArray());
        var actions = observer.Entered.OfType<IActivityActionObservationContext>().ToArray();
        Assert.AreEqual(2, observer.Entered.OfType<IActivityPredicateObservationContext>()
            .Count(context => context.OwnerId == "Graph" && context.Operation == ActivityOperationKind.Initialize));
        Assert.AreEqual(1, observer.Aggregate.Count(operation => operation == "initialize"));
        Assert.AreEqual(2, actions.Count(context => context.OwnerId == "Graph" && context.Operation == ActivityOperationKind.Finalize));
        var workId = definitions.Nodes.Single(node => node.Value == "work").Key;
        var executeId = definitions.Nodes.Single(node => node.Value == "execute").Key;
        Assert.AreEqual(2, actions.Count(context => context.OwnerId == workId && context.Operation == ActivityOperationKind.Initialize));
        Assert.AreEqual(2, actions.Count(context => context.OwnerId == workId && context.Operation == ActivityOperationKind.Finalize));
        Assert.IsTrue(actions.Any(context => context.OwnerId == executeId && context.Operation == ActivityOperationKind.Execute));
        Assert.IsTrue(actions.All(context => context.Event is Start && context.EventId.HasValue));
        Assert.AreEqual(1, actions.Select(context => context.EventId).Distinct().Count());
    }

    [TestMethod]
    public async Task DeclinedInitializerReportsItsOwnResultAndSkipsLaterRegistrations()
    {
        var definitions = await Definitions("observed-declined-initializer");
        await Locate("observed-declined-initializer").SendAsync(new Declined());
        AssertIdentity(definitions);
        CollectionAssert.AreEqual(new[] { "declined-0" }, invoked.ToArray());
        Assert.AreEqual(1, observer.Entered.Count);
        var initializer = (IActivityPredicateObservationContext)observer.Entered.Single();
        Assert.AreEqual(ActivityOperationKind.Initialize, initializer.Operation);
        Assert.AreEqual(false, observer.Results[initializer.Identifier]);
        Assert.AreEqual(0, declinedActions);
    }

    [TestMethod]
    public async Task DefaultInitializerUsesPredicateHookAndFalsePreventsNodeExecution()
    {
        var definitions = await Definitions("observed-declined-default-initializer");
        await Locate("observed-declined-default-initializer").SendAsync(new Initialize());
        AssertIdentity(definitions);
        CollectionAssert.AreEqual(new[] { "default-0" }, invoked.ToArray());
        var predicates = observer.Entered.OfType<IActivityPredicateObservationContext>().ToArray();
        Assert.AreEqual(1, predicates.Length);
        Assert.IsTrue(predicates.All(predicate => predicate.Operation == ActivityOperationKind.DefaultInitialize));
        Assert.AreEqual(false, observer.Results[predicates[0].Identifier]);
        Assert.AreEqual(1, observer.Aggregate.Count(operation => operation == "initialize"));
        Assert.AreEqual(0, declinedActions);
    }

    [TestMethod]
    public async Task RepeatedRegistrationContinuationFailsWithoutRepeatingTransformation()
    {
        observer.DuplicateTransformation = true;
        var result = await Locate("observed-pipeline").SendAsync(new TokensInput().Add(12));
        Assert.AreEqual(EventStatus.Failed, result.Status);
        Assert.AreEqual(1, transformations);
    }

    [TestMethod]
    public async Task RegisteredActionFailureCannotBeSwallowedByItsObserver()
    {
        var definitions = await Definitions("observed-failed-action");
        observer.FailureToSwallow = originalFailure;
        var result = await Locate("observed-failed-action").SendAsync(new Initialize());
        Assert.AreEqual(EventStatus.Failed, result.Status);
        Assert.AreSame(originalFailure, observer.SwallowedFailure);
        AssertIdentity(definitions);
    }
}
