using Activity.IntegrationTests.Classes.Events;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    #region Base Activities
    
    public class BaseActivity : IActivity
    {
        public static bool Action2Executed = false;
        public static bool Action3Executed = false;
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow("action1")
                .AddControlFlow<StructuredActivityForTest>()
                .AddControlFlow<ParallelActivityForTest>()
                .AddControlFlow<IterativeActivityForTest>()
            )
            .AddAction("action1", async c => c.Output(42), b => b
                .AddFlow<int>(DecisionNode<int>.Name)
            )
            .AddDecision<int>(b => b
                .AddFlow("action2", b => b.AddGuard(async t => t.Token == 42))
                .AddElseFlow("action3")
            )
            .AddAction("action2", async c => Action2Executed = true)
            .AddAction("action3", async c => Action3Executed = true)
            .AddStructuredActivity<StructuredActivityForTest>(b => b
                .AddInitial(b => b
                    .AddControlFlow("action1")
                )
                .AddAction("action1", async c => { })
            )
            .AddParallelActivity<int, ParallelActivityForTest>(b => b
                .AddInitial(b => b
                    .AddControlFlow("action1")
                )
                .AddAction("action1", async c => { })
            )
            .AddIterativeActivity<int, IterativeActivityForTest>(b => b
                .AddInitial(b => b
                    .AddControlFlow("action1")
                )
                .AddAction("action1", async c => { })
            )
        ;
    }

    public class ActivityWithControlDecision : IActivity
    {
        public static bool NormalFlowExecuted = false;
        public static bool AlternativeFlowExecuted = false;
        
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow(ControlDecisionNode.Name)
            )
            .AddControlDecision(b => b
                .AddFlow("normal", b => b.AddGuard(async c => true))
                .AddElseFlow("alternative")
            )
            .AddAction("normal", async c => NormalFlowExecuted = true)
            .AddAction("alternative", async c => AlternativeFlowExecuted = true)
        ;
    }

    public class ActivityWithDataStore : IActivity
    {
        public static bool DataStoreExecuted = false;
        public static int TokenCount = 0;
        
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow("generate")
            )
            .AddAction("generate", async c => c.OutputRange(new[] { 1, 2, 3 }), b => b
                .AddFlow<int>(DataStoreNode.Name)
            )
            .AddDataStore(b => b
                .AddFlow<int>("final")
            )
            .AddAction("final", async c =>
            {
                DataStoreExecuted = true;
                TokenCount = c.GetTokensOfType<int>().Count();
            })
        ;
    }

    public class ActivityWithJoinFork : IActivity
    {
        public static bool FinalActionExecuted = false;
        public static int ForkCount = 0;
        
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow("action1")
            )
            .AddAction("action1", async c => { }, b => b
                .AddControlFlow(ForkNode.Name)
            )
            .AddFork(b => b
                .AddControlFlow("action2")
                .AddControlFlow("action3")
            )
            .AddAction("action2", async c => ForkCount++)
            .AddAction("action3", async c => ForkCount++, b => b
                .AddControlFlow(JoinNode.Name)
            )
            .AddJoin(b => b
                .AddControlFlow("final")
            )
            .AddAction("final", async c => FinalActionExecuted = true)
        ;
    }

    public class ActivityWithMerge : IActivity
    {
        public static bool FinalActionExecuted = false;
        
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow("action1")
                .AddControlFlow("action2")
            )
            .AddAction("action1", async c => { }, b => b
                .AddControlFlow(MergeNode.Name)
            )
            .AddAction("action2", async c => { }, b => b
                .AddControlFlow(MergeNode.Name)
            )
            .AddMerge(b => b
                .AddControlFlow("final")
            )
            .AddAction("final", async c => FinalActionExecuted = true)
        ;
    }

    public class ActivityWithAcceptEvent : IActivity
    {
        public static bool AcceptEventExecuted = false;
        public static bool TestActionExecuted = false;
        
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow<SomeEventAcceptEventAction>()
                .AddControlFlow("final")
            )
            .AddAcceptEventAction<SomeEvent, SomeEventAcceptEventAction>(
                b => b.AddControlFlow("final")
            )
            .AddAction("final", async c => { })
        ;
    }

    public class ActivityWithTimeEvent : IActivity
    {
        public static bool TimeEventExecuted = false;
        
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow("timeEvent")
                .AddControlFlow("final")
            )
            .AddTimeEventAction<TimeoutEvent>(
                "timeEvent",
                async c => TimeEventExecuted = true,
                b => b.AddControlFlow("final")
            )
            .AddAction("final", async c => { })
        ;
    }

    public class TimeoutEvent : TimeEvent
    {
        protected override DateTime GetTriggerTime(DateTime startedAt) 
            => startedAt.AddMilliseconds(10);
    }

    public class ActivityWithSendEvent : IActivity
    {
        public static bool InitialExecuted = false;
        
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow("send")
            )
            .AddSendEventAction(
                "send",
                async c =>
                {
                    InitialExecuted = true;
                    return new SomeEvent();
                },
                async c => new BehaviorId("", "", ""),
                b => b.AddControlFlow(FinalNode.Name)
            )
            .AddFinal()
        ;
    }

    public class StructuredActivityForTest : IStructuredActivityNodeDefinition
    {
        public static bool ExecutionCompleted = false;
        
        public static void Build(IStructuredActivityBuilder builder) => builder
            .AddInitial(b => b
                .AddControlFlow("action1")
            )
            .AddAction("action1", async c => ExecutionCompleted = true)
        ;
    }

    public class ParallelActivityForTest : IStructuredActivityNodeDefinition
    {
        public static int ExecutionCount = 0;
        
        public static void Build(IStructuredActivityBuilder builder) => builder
            .AddInitial(b => b.AddControlFlow("action1"))
            .AddAction("action1", async c => ExecutionCount++)
        ;
    }

    public class IterativeActivityForTest : IStructuredActivityNode
    {
        public static int ExecutionCount = 0;
        
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b.AddControlFlow("action1"))
            .AddAction("action1", async c => ExecutionCount++)
        ;
    }
    
    public class TestAction(IInputTokens<IInherited> input) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
            => ActivityWithAcceptEvent.TestActionExecuted = true;
    }

    public class SomeEventAcceptEventAction(
        IOutputTokens<int> intOutput,
        IOutputTokens<BaseEvent> someEventOutput
    ) : IAcceptEventActionNode<BaseEvent>
    {
        public Task ExecuteAsync(BaseEvent @event, CancellationToken cancellationToken)
        {
            ActivityWithAcceptEvent.AcceptEventExecuted = true;

            intOutput.Add(42);
            
            someEventOutput.Add(@event);
            
            return Task.CompletedTask;
        }
    }

    #endregion
    
    [TestClass]
    public class Override : StateflowsTestClass
    {
        public static bool Action4Executed = false;
        
        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddActivities(b => b
                    // UseActivity + UseDecision + UseFlow tests
                    .AddActivity("addedGuard", b => b
                        .UseActivity<BaseActivity>(b => b
                            .UseDecision<int>(b => b
                                .UseFlow("action2", b => b.AddGuard(Guards.Deny))
                            )
                        )
                    )
                    
                    // UseActivity + UseAction tests
                    .AddActivity("addedAction", b => b
                        .UseActivity<BaseActivity>(b => b
                            .UseAction("action2", b => b
                                .AddControlFlow("action4")
                            )
                            .AddAction("action4", async c => Action4Executed = true)
                        )
                    )

                    // UseActivity + UseInitial tests
                    .AddActivity("overrideInitial", b => b
                        .UseActivity<BaseActivity>(b => b
                            .UseInitial(b => b
                                .AddControlFlow("newStart")
                            )
                            .AddAction("newStart", async c => { }, b => b
                                .AddControlFlow("action1")
                            )
                        )
                    )

                    // UseActivity + UseControlDecision + UseFlow tests
                    .AddActivity("overrideControlDecision", b => b
                        .UseActivity<ActivityWithControlDecision>(b => b
                            .UseControlDecision(b => b
                                .UseFlow("normal", b => b.AddGuard(Guards.Deny))
                            )
                        )
                    )

                    // UseActivity + UseDataStore tests
                    .AddActivity("overrideDataStore", b => b
                        .UseActivity<ActivityWithDataStore>(b => b
                            .UseDataStore(b => b
                                .UseFlow<int>("final", b => b.AddGuard(async t => t.Token > 1))
                            )
                        )
                    )

                    // UseActivity + UseJoin tests
                    .AddActivity("overrideJoin", b => b
                        .UseActivity<ActivityWithJoinFork>(b => b
                            .UseJoin(b => b
                                .AddControlFlow("preFinal")
                            )
                            .AddAction("preFinal", async c => { }, b => b
                                .AddControlFlow("final")
                            )
                        )
                    )

                    // UseActivity + UseFork tests
                    .AddActivity("overrideFork", b => b
                        .UseActivity<ActivityWithJoinFork>(b => b
                            .UseFork(b => b
                                .AddControlFlow("preBranch1")
                                .AddControlFlow("preBranch2")
                            )
                            .AddAction("preBranch1", async c => { }, b => b
                                .AddControlFlow("action2")
                            )
                            .AddAction("preBranch2", async c => { }, b => b
                                .AddControlFlow("action3")
                            )
                        )
                    )

                    // UseActivity + UseMerge tests
                    .AddActivity("overrideMerge", b => b
                        .UseActivity<ActivityWithMerge>(b => b
                            .UseMerge(b => b
                                .UseControlFlow("final", b => b
                                    .AddGuard(Guards.Deny)
                                )
                            )
                        )
                    )

                    // UseActivity + UseAcceptEventAction tests
                    .AddActivity("overrideAcceptEvent", b => b
                        .UseActivity<ActivityWithAcceptEvent>(b => b
                            .UseAcceptEventAction<SomeEvent, SomeEventAcceptEventAction>(b => b
                                .UseControlFlow("final", b => b
                                    .AddGuard(Guards.Allow)
                                )
                                .AddFlow<int>("x")
                            )
                            .AddAction("x", async c =>
                            {
                                var tokens = c.GetTokensOfType<int>();
                                if (tokens.Count() == 1 && tokens.FirstOrDefault() != 42)
                                {
                                    throw new InvalidOperationException("Token not received");
                                }
                                else
                                {
                                    c.Behavior.Publish(tokens.FirstOrDefault());
                                }
                            })
                        )
                    )

                    // UseActivity + UseAcceptEventAction + ChangeAcceptedEvent + tests
                    .AddActivity("changeAcceptEvent", b => b
                        .UseActivity<ActivityWithAcceptEvent>(b => b
                            .UseAcceptEventAction<SomeEvent, SomeEventAcceptEventAction>(b => b
                                .ChangeAcceptedEvent<SomeInheritedEvent>()
                                .AddFlow<SomeInheritedEvent, TestAction>(b => b
                                    .AddGuard(async c => c.Token is not null)
                                    // .AddTransformation(async c => c.Token as SomeInheritedEvent)
                                )
                                .AddFlow<int, MergeNode>()
                            )
                            .AddAction<TestAction>()
                            .AddMerge(b => b
                                .AddFlow<int, FinalNode>()
                            )
                            .AddFinal()
                        )
                    )

                    // UseActivity + UseTimeEventAction tests
                    .AddActivity("overrideTimeEvent", b => b
                        .UseActivity<ActivityWithTimeEvent>(b => b
                            .UseTimeEventAction<TimeoutEvent>("timeEvent", b => b
                                .UseControlFlow("final", b => b
                                    .AddGuard(Guards.Allow)
                                )
                            )
                        )
                    )

                    // UseActivity + UseSendEventAction tests
                    .AddActivity("overrideSendEvent", b => b
                        .UseActivity<ActivityWithSendEvent>(b => b
                            .UseSendEventAction<SomeEvent>("send", b => b
                                .UseControlFlow(FinalNode.Name, b => b
                                    .AddGuard(Guards.Allow)
                                )
                            )
                        )
                    )

                    // UseActivity + UseStructuredActivity tests
                    .AddActivity("overrideStructuredActivity", b => b
                        .UseActivity<BaseActivity>(b => b
                            .UseStructuredActivity<StructuredActivityForTest>(b => b
                                .UseAction("action1", b => b
                                    .AddControlFlow("action2")
                                )
                                .AddAction("action2", async c => { })
                            )
                        )
                    )

                    // UseActivity + UseParallelActivity tests
                    .AddActivity("overrideParallelActivity", b => b
                        .UseActivity<BaseActivity>(b => b
                            .UseParallelActivity<int, ParallelActivityForTest>(b => b
                                .UseAction("action1", b => b
                                    .AddControlFlow("action2")
                                )
                                .AddAction("action2", async c => { })
                            )
                        )
                    )

                    // UseActivity + UseIterativeActivity tests
                    .AddActivity("overrideIterativeActivity", b => b
                        .UseActivity<BaseActivity>(b => b
                            .UseIterativeActivity<int, IterativeActivityForTest>(b => b
                                .UseAction("action1", b => b
                                    .AddControlFlow("action2")
                                )
                                .AddAction("action2", async c => { })
                            )
                        )
                    )

                    // // UseActivity + UseInput tests
                    // .AddActivity("overrideInput", b => b
                    //     .UseActivity<BaseActivity>(b => b
                    //         .UseInput(b => b
                    //             .AddControlFlow("action1")
                    //         )
                    //     )
                    // )
                )
                ;
        }

        [TestMethod]
        public async Task UseDecisionWithGuardOverride()
        {
            BaseActivity.Action2Executed = false;
            BaseActivity.Action3Executed = false;
            Action4Executed = false;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("addedGuard", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsFalse(BaseActivity.Action2Executed);
            Assert.IsTrue(BaseActivity.Action3Executed);
            Assert.IsFalse(Action4Executed);
        }

        [TestMethod]
        public async Task UseActionWithFlow()
        {
            BaseActivity.Action2Executed = false;
            BaseActivity.Action3Executed = false;
            Action4Executed = false;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("addedAction", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(BaseActivity.Action2Executed);
            Assert.IsFalse(BaseActivity.Action3Executed);
            Assert.IsTrue(Action4Executed);
        }

        [TestMethod]
        public async Task UseInitialOverride()
        {
            BaseActivity.Action2Executed = false;
            BaseActivity.Action3Executed = false;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideInitial", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(BaseActivity.Action2Executed, "Action2 should be executed");
            Assert.IsFalse(BaseActivity.Action3Executed, "Action3 should be executed");
        }

        [TestMethod]
        public async Task UseControlDecisionOverride()
        {
            ActivityWithControlDecision.NormalFlowExecuted = false;
            ActivityWithControlDecision.AlternativeFlowExecuted = false;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideControlDecision", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsFalse(ActivityWithControlDecision.NormalFlowExecuted);
            Assert.IsTrue(ActivityWithControlDecision.AlternativeFlowExecuted);
        }

        [TestMethod]
        public async Task UseDataStoreOverride()
        {
            ActivityWithDataStore.DataStoreExecuted = false;
            ActivityWithDataStore.TokenCount = 0;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideDataStore", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(ActivityWithDataStore.DataStoreExecuted);
            Assert.IsTrue(ActivityWithDataStore.TokenCount >= 2, "Should have at least 2 tokens (2 and 3)");
        }

        [TestMethod]
        public async Task UseJoinOverride()
        {
            ActivityWithJoinFork.FinalActionExecuted = false;
            ActivityWithJoinFork.ForkCount = 0;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideJoin", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(ActivityWithJoinFork.FinalActionExecuted);
            Assert.AreEqual(2, ActivityWithJoinFork.ForkCount);
        }

        [TestMethod]
        public async Task UseForkOverride()
        {
            ActivityWithJoinFork.FinalActionExecuted = false;
            ActivityWithJoinFork.ForkCount = 0;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideFork", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(ActivityWithJoinFork.FinalActionExecuted);
        }

        [TestMethod]
        public async Task UseMergeOverride()
        {
            ActivityWithMerge.FinalActionExecuted = false;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideMerge", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsFalse(ActivityWithMerge.FinalActionExecuted);
        }

        [TestMethod]
        public async Task UseAcceptEventActionOverride()
        {
            ActivityWithAcceptEvent.AcceptEventExecuted = false;
            var counter = 0;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideAcceptEvent", "x"), out var a))
            {
                await using var watcher = await a.WatchAsync<int>(t => counter++);
                await a.SendAsync(new Initialize());
                await a.SendAsync(new SomeEvent());
            }

            // Even without sending event, activity should complete
            // The override should work without errors
            Assert.IsNotNull(a);
            Assert.AreEqual(1, counter);
        }

        [TestMethod]
        public async Task ChangeAcceptEventActionOverride()
        {
            ActivityWithAcceptEvent.AcceptEventExecuted = false;
            ActivityWithAcceptEvent.TestActionExecuted = false;
            SendResult? result1 = null;
            SendResult? result2 = null;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("changeAcceptEvent", "x"), out var a))
            {
                result1 = await a.SendAsync(new SomeEvent());
                result2 = await a.SendAsync(new SomeInheritedEvent());
            }

            // Even without sending event, activity should complete
            // The override should work without errors
            Assert.IsNotNull(a);
            
            Assert.AreEqual(EventStatus.NotConsumed, result1?.Status);
            Assert.AreEqual(EventStatus.Consumed, result2?.Status);
            Assert.IsTrue(ActivityWithAcceptEvent.TestActionExecuted);
        }

        [TestMethod]
        public async Task UseTimeEventActionOverride()
        {
            ActivityWithTimeEvent.TimeEventExecuted = false;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideTimeEvent", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
                await Task.Delay(100); // Wait for timeout
            }

            Assert.IsNotNull(a);
        }

        [TestMethod]
        public async Task UseSendEventActionOverride()
        {
            ActivityWithSendEvent.InitialExecuted = false;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideSendEvent", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(ActivityWithSendEvent.InitialExecuted);
        }

        [TestMethod]
        public async Task UseStructuredActivityOverride()
        {
            StructuredActivityForTest.ExecutionCompleted = false;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideStructuredActivity", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsNotNull(a);
        }

        [TestMethod]
        public async Task UseParallelActivityOverride()
        {
            ParallelActivityForTest.ExecutionCount = 0;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideParallelActivity", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsNotNull(a);
        }

        [TestMethod]
        public async Task UseIterativeActivityOverride()
        {
            IterativeActivityForTest.ExecutionCount = 0;
            
            if (ActivityLocator.TryLocateActivity(new ActivityId("overrideIterativeActivity", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsNotNull(a);
        }

        // [TestMethod]
        // public async Task UseInputOverride()
        // {
        //     BaseActivity.Action2Executed = false;
        //     BaseActivity.Action3Executed = false;
        //     
        //     if (ActivityLocator.TryLocateActivity(new ActivityId("overrideInput", "x"), out var a))
        //     {
        //         await a.SendAsync(new Initialize());
        //     }
        //
        //     Assert.IsNotNull(a);
        // }
    }
}