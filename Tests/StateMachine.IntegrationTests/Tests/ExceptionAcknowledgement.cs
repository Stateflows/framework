using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;
using Stateflows.Entities;
using Stateflows.Entities.Attributes;
using Stateflows.StateMachines.Context.Interfaces;

namespace StateMachine.IntegrationTests.Tests
{
    public interface IAcknowledgementTemplate
    {
        [Field]
        string Value { get; set; }
    }

    [TestClass]
    public class ExceptionAcknowledgement : StateflowsTestClass
    {
        private readonly Exception original = new InvalidOperationException("operation failed");
        private readonly Exception secondary = new ApplicationException("handler failed");
        private readonly List<(int Handler, Exception Exception)> calls = [];
        private bool acknowledged;
        private bool throwFromHandler;
        private bool graphHandlerRan;
        private bool genericGraphHandlerRan;
        private int finalizerCalls;
        private bool suspendHandlers;
        private readonly TaskCompletionSource handlerEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource releaseHandler = new(TaskCreationOptions.RunContinuationsAsynchronously);

        [TestInitialize]
        public override void Initialize() => base.Initialize();

        [TestCleanup]
        public override void Cleanup() => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            var handlers = Enumerable.Range(1, 3).Select(i => new RecordingHandler(this, i)).ToArray();
            builder.AddActions(actions => actions.AddAction("diagnostics", _ => throw original,
                action =>
                {
                    foreach (var handler in handlers)
                        action.AddExceptionHandler(_ => handler);
                }));
            builder.AddStateMachines(machines => machines.AddStateMachine("diagnostics", machine =>
            {
                machine.AddInitialState("initial", state => state.AddOnEntry(_ => throw original));
                foreach (var handler in handlers)
                    machine.AddExceptionHandler((_, _) => handler);
            }));
            builder.AddActivities(activities => activities
                .AddActivity("diagnostics", activity =>
                {
                    activity.AddInitial(initial => initial.AddControlFlow("faulty"))
                        .AddAction("faulty", _ => throw original);
                    foreach (var handler in handlers)
                        activity.AddExceptionHandler(_ => handler);
                })
                .AddActivity("graph-diagnostics", activity => activity
                    .AddInitial(initial => initial.AddControlFlow("faulty"))
                    .AddAction("faulty", context =>
                    {
                        throw original;
                    }, node => node.AddExceptionHandler<Exception>(_ =>
                    {
                        graphHandlerRan = true;
                        return Task.CompletedTask;
                    })))
                .AddActivity("graph-guard-diagnostics", activity => activity
                    .AddInitial(initial => initial.AddControlFlow("source"))
                    .AddAction("source", _ => Task.CompletedTask, node => node
                        .AddExceptionHandler<Exception>(_ =>
                        {
                            graphHandlerRan = true;
                            return Task.CompletedTask;
                        })
                        .AddControlFlow<FinalNode>(flow => flow.AddGuard(context =>
                        {
                            throw original;
                        })))
                    .AddFinal())
                .AddActivity("graph-transformation-diagnostics", activity => activity
                    .AddInitial(initial => initial.AddControlFlow("source"))
                    .AddAction("source", context =>
                    {
                        context.Output(42);
                        return Task.CompletedTask;
                    }, node => node
                        .AddExceptionHandler<Exception>(_ =>
                        {
                            graphHandlerRan = true;
                            return Task.CompletedTask;
                        })
                        .AddFlow<int>("target", flow => flow.AddTransformation<int>(context =>
                        {
                            throw original;
                        })))
                    .AddAction("target", _ => Task.CompletedTask))
                .AddActivity("graph-specific-diagnostics", activity => activity
                    .AddInitial(initial => initial.AddControlFlow("source"))
                    .AddAction("source", _ => Task.CompletedTask, node => node
                        .AddExceptionHandler<Exception>(_ =>
                        {
                            genericGraphHandlerRan = true;
                            return Task.CompletedTask;
                        })
                        .AddExceptionHandler<InvalidOperationException>(_ =>
                        {
                            graphHandlerRan = true;
                            return Task.CompletedTask;
                        })
                        .AddControlFlow<FinalNode>(flow => flow.AddGuard(context =>
                        {
                            throw original;
                        })))
                    .AddFinal())
                .AddActivity("finalizer-diagnostics", activity =>
                {
                    activity.AddInitial(initial => initial.AddControlFlow<FinalNode>())
                        .AddFinal()
                        .AddFinalizer(_ =>
                        {
                            finalizerCalls++;
                            throw original;
                        });
                    foreach (var handler in handlers)
                        activity.AddExceptionHandler(_ => handler);
                }));
            builder.AddEntities(entities => entities.AddEntity<IAcknowledgementTemplate>("diagnostics", entity =>
            {
                entity.AddDefaultInitializer(context =>
                {
                    context.Entity.Value = "partial";
                    throw original;
                });
                foreach (var handler in handlers)
                    entity.AddExceptionHandler((_, _) => handler);
            }));
        }

        private bool Record(int handler, object context, Exception exception)
        {
            calls.Add((handler, exception));
            if (handler == 2 && throwFromHandler)
                throw secondary;
            return handler == 1 && acknowledged;
        }

        private Task<bool> RecordAsync(int handler, object context, Exception exception)
            => suspendHandlers
                ? RecordAfterSuspensionAsync(handler, context, exception)
                : Task.FromResult(Record(handler, context, exception));

        private async Task<bool> RecordAfterSuspensionAsync(int handler, object context, Exception exception)
        {
            if (handler == 1)
            {
                handlerEntered.TrySetResult();
                await releaseHandler.Task;
            }
            await Task.Yield();
            return Record(handler, context, exception);
        }

        private async Task<(EventStatus Status, List<Exception> Exceptions)> ExecuteAsync(string type, string name = "diagnostics")
        {
            ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            var processor = ServiceProvider.GetServices<IEventProcessor>()
                .Single(processor => processor.BehaviorTypes.Contains(type));
            var exceptions = new List<Exception>();
            var status = await processor.ProcessEventAsync(
                new BehaviorId(type, name, "instance"), new Initialize().ToEventHolder(), exceptions);
            return (status, exceptions);
        }

        [TestMethod]
        [DataRow(BehaviorType.Action, false)]
        [DataRow(BehaviorType.Action, true)]
        [DataRow(BehaviorType.Activity, false)]
        [DataRow(BehaviorType.Activity, true)]
        [DataRow(BehaviorType.StateMachine, false)]
        [DataRow(BehaviorType.StateMachine, true)]
        [DataRow(BehaviorType.Entity, false)]
        [DataRow(BehaviorType.Entity, true)]
        public async Task Acknowledgement_DoesNotRecoverExecutionOrPersistPartialState(string type, bool acknowledge)
        {
            acknowledged = acknowledge;
            var (status, exceptions) = await ExecuteAsync(type);

            Assert.AreEqual(EventStatus.Failed, status);
            Assert.AreEqual(1, exceptions.Count);
            Assert.AreSame(original, exceptions.Single(), string.Join(Environment.NewLine, exceptions));
            Assert.IsTrue(calls.Count >= 3);
            Assert.IsTrue(calls.All(call => ReferenceEquals(original, call.Exception)));
            CollectionAssert.AreEqual(
                Enumerable.Range(0, calls.Count).Select(i => i % 3 + 1).ToArray(),
                calls.Select(call => call.Handler).ToArray());
            ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            var stored = await ServiceProvider.GetRequiredService<IStateflowsStorage>()
                .HydrateAsync(new BehaviorId(type, "diagnostics", "instance"));
            Assert.AreNotEqual(BehaviorStatus.Initialized, stored.Status);
            Assert.IsFalse(stored.Values.ContainsKey("$field:Value"));
        }

        [TestMethod]
        [DataRow(BehaviorType.Action)]
        [DataRow(BehaviorType.Activity)]
        [DataRow(BehaviorType.StateMachine)]
        [DataRow(BehaviorType.Entity)]
        public async Task HandlerFailure_IsRecordedSeparately_AndLaterHandlersRun(string type)
        {
            acknowledged = true;
            throwFromHandler = true;
            var (status, exceptions) = await ExecuteAsync(type);

            Assert.AreEqual(EventStatus.Failed, status);
            Assert.IsTrue(exceptions.Any(exception => ReferenceEquals(original, exception)));
            Assert.IsTrue(exceptions.Any(exception => ReferenceEquals(secondary, exception)));
            Assert.IsTrue(calls.Any(call => call.Handler == 3));
            Assert.AreEqual(2, exceptions.Count);
        }

        [TestMethod]
        [DataRow(BehaviorType.Action)]
        [DataRow(BehaviorType.Activity)]
        [DataRow(BehaviorType.StateMachine)]
        [DataRow(BehaviorType.Entity)]
        public async Task SuspendedHandlers_AreAwaitedInOrder_AndAsyncFailuresDoNotStopDispatch(string type)
        {
            suspendHandlers = true;
            acknowledged = true;
            throwFromHandler = true;
            var execution = ExecuteAsync(type);
            try
            {
                await handlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.IsFalse(execution.IsCompleted);
                Assert.AreEqual(0, calls.Count);
            }
            finally
            {
                releaseHandler.TrySetResult();
                await execution.WaitAsync(TimeSpan.FromSeconds(10));
            }

            var (status, exceptions) = await execution;
            Assert.AreEqual(EventStatus.Failed, status);
            Assert.AreEqual(2, exceptions.Count);
            Assert.IsTrue(exceptions.Any(exception => ReferenceEquals(original, exception)));
            Assert.IsTrue(exceptions.Any(exception => ReferenceEquals(secondary, exception)));
            Assert.IsTrue(calls.Count >= 3);
            CollectionAssert.AreEqual(
                Enumerable.Range(0, calls.Count).Select(i => i % 3 + 1).ToArray(),
                calls.Select(call => call.Handler).ToArray());
        }

        [TestMethod]
        [DataRow("graph-diagnostics")]
        [DataRow("graph-guard-diagnostics")]
        [DataRow("graph-transformation-diagnostics")]
        [DataRow("graph-specific-diagnostics")]
        public async Task GraphHandler_AcknowledgesWithoutRecoveringTheEvent(string name)
        {
            var (status, exceptions) = await ExecuteAsync(BehaviorType.Activity, name);

            Assert.IsTrue(graphHandlerRan);
            Assert.IsFalse(genericGraphHandlerRan);
            Assert.AreEqual(EventStatus.Failed, status);
            Assert.AreSame(original, exceptions.Single());
        }

        [TestMethod]
        public async Task ActivityFinalizerFailure_IsAcknowledged_AndInvokedOnlyOnce()
        {
            acknowledged = true;
            var (status, exceptions) = await ExecuteAsync(BehaviorType.Activity, "finalizer-diagnostics");

            Assert.AreEqual(1, finalizerCalls);
            Assert.AreEqual(EventStatus.Failed, status);
            Assert.AreSame(original, exceptions.Single());
            Assert.IsTrue(calls.Count >= 3);
            Assert.IsTrue(calls.All(call => ReferenceEquals(original, call.Exception)));
        }

        [TestMethod]
        public async Task AsyncHandlers_AreAwaited_AndKeepRunningAfterAcknowledgementAndFailure()
        {
            var context = new StateflowsContext();
            var completed = new List<int>();
            var handled = await ExecutionExceptionHandling.HandleAsync(
                context, new[] { 1, 2, 3 }, async handler =>
                {
                    await Task.Yield();
                    completed.Add(handler);
                    if (handler == 2)
                        throw secondary;
                    return handler == 1;
                }, "test", NullLogger.Instance);

            Assert.IsTrue(handled);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, completed);
            Assert.AreSame(secondary, context.ExecutionExceptions.Single());
        }

        [TestMethod]
        public void DynamicDelegates_PreserveOriginalExceptionAndThrowingFrame()
        {
            System.Action action = ThrowOriginal;
            var exception = Assert.Throws<InvalidOperationException>(() => action.InvokeDelegate([]));
            Assert.AreSame(original, exception);
            StringAssert.Contains(exception.StackTrace!, nameof(ThrowOriginal));
        }

        [TestMethod]
        public void DynamicDelegateValueFactories_DoNotReplaceFailuresWithNullOrAggregateExceptions()
        {
            System.Action<object> action = _ => Assert.Fail("Must not invoke the action.");
            Assert.AreSame(original, Assert.Throws<InvalidOperationException>(
                () => action.InvokeDelegate([() => throw original])));
            Assert.AreSame(original, Assert.Throws<InvalidOperationException>(
                () => action.InvokeDelegate([() => Task.FromException<object>(original)])));
        }

        [TestMethod]
        public void GraphHandlerMatching_RejectsDerivedHandlersForBaseExceptions()
        {
            var handler = new Stateflows.Activities.Models.Node
            {
                Type = NodeType.ExceptionHandler,
                ExceptionType = typeof(InvalidOperationException)
            };
            var node = new Stateflows.Activities.Models.Node
            {
                Edges = [new Stateflows.Activities.Models.Edge { Target = handler }]
            };

            Assert.IsNull(node.FindExceptionHandler(new Exception()));
            Assert.AreSame(handler, node.FindExceptionHandler(original));
        }

        private void ThrowOriginal() => throw original;

        [TestMethod]
        public async Task BoundaryFailuresWithoutHooks_AreRecordedAlongsideEarlierFailures()
        {
            ServiceProvider.GetRequiredService<ITenantAccessor>().CurrentTenantId =
                ServiceProvider.GetRequiredService<IStateflowsTenantProvider>().GetCurrentTenantId();
            StateflowsContext? working = null;
            var exceptions = new List<Exception> { secondary };
            var status = await ContextExecution.RunAsync(
                ServiceProvider.GetRequiredService<IStateflowsStorage>(),
                new ActionId("diagnostics", "boundary"), exceptions, execution =>
                {
                    working = execution.Context;
                    throw original;
                });

            Assert.AreEqual(EventStatus.Failed, status);
            Assert.IsNotNull(working);
            Assert.AreSame(original, exceptions[1]);
            Assert.AreEqual(2, exceptions.Count);
        }

        private sealed class RecordingHandler(ExceptionAcknowledgement owner, int number)
            : StateMachineExceptionHandler, IActionExceptionHandler, IActivityExceptionHandler, IEntityExceptionHandler
        {
            public Task<bool> OnActionExceptionAsync(IActionDelegateContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public override Task<bool> OnStateMachineInitializationExceptionAsync(IStateMachineInitializationContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public override Task<bool> OnStateMachineFinalizationExceptionAsync(IStateMachineActionContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public override Task<bool> OnTransitionGuardExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public override Task<bool> OnTransitionEffectExceptionAsync<TEvent>(ITransitionContext<TEvent> context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public override Task<bool> OnStateInitializationExceptionAsync(IStateActionContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public override Task<bool> OnStateFinalizationExceptionAsync(IStateActionContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public override Task<bool> OnStateEntryExceptionAsync(IStateActionContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public override Task<bool> OnStateExitExceptionAsync(IStateActionContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public Task<bool> OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public Task<bool> OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public Task<bool> OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public Task<bool> OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public Task<bool> OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public Task<bool> OnFlowGuardExceptionAsync<TToken>(IGuardContext<TToken> context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public Task<bool> OnFlowTransformationExceptionAsync<TToken, TTransformedToken>(ITransformationContext<TToken> context, Exception exception)
                => owner.RecordAsync(number, context, exception);
            public Task<bool> OnEntityInitializationExceptionAsync(IEntityInitializationExceptionContext context, Exception exception)
                => owner.RecordAsync(number, context.Behavior, exception);
            public Task<bool> OnMutationExceptionAsync<TTemplate, TMutationEvent>(IMutationObservationContext<TTemplate, TMutationEvent> context, Exception exception)
                => owner.RecordAsync(number, context.Behavior, exception);
            public Task<bool> OnComputedFieldExceptionAsync<TTemplate, TField>(IComputedFieldObservationContext<TTemplate> context, Exception exception)
                => owner.RecordAsync(number, context.Behavior, exception);
            public Task<bool> OnProjectionExceptionAsync<TTemplate, TProjection>(IProjectionObservationContext<TTemplate> context, Exception exception)
                => owner.RecordAsync(number, context.Behavior, exception);
        }
    }
}
