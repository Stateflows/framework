using Activity.IntegrationTests.Classes.Actions;
using Activity.IntegrationTests.Classes.Events;
using Activity.IntegrationTests.Classes.Flow;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities;
using Stateflows.Common;

namespace Activity.IntegrationTests.Classes.Activities
{
    internal class EverythingActivity : IActivity
    {
        public static void Build(IActivityBuilder builder)
        {
            builder
                .AddInitial(b => b
                    .AddControlFlow("action1", b => b
                        .AddGuard(async c => true)
                    )
                    .AddControlFlow<Action1>(b => b
                        .AddGuard(async c => true)
                    )
                    .AddControlFlow<ControlFlow>("action1")
                    .AddControlFlow<ControlFlow, Action1>()
                )
                
                .AddInput(b => b
                    .AddFlow<int>("action1")
                    .AddFlow<int, Action1>()
                    .AddFlow<int, IntFlow>("action1")
                    .AddFlow<int, IntStringFlow>("action1")
                    .AddFlow<int, IntFlow, Action1>()
                    .AddFlow<int, IntStringFlow, Action1>()
                )
                
                .AddOutput()
                .AddFinal()

                .AddDataStore("dataStore1", b => b
                    .AddFlow<int>("action1")
                    .AddFlow<int, Action1>()
                    .AddFlow<int, IntFlow>("action1")
                    .AddFlow<int, IntStringFlow>("action1")
                    .AddFlow<int, IntFlow, Action1>()
                    .AddFlow<int, IntStringFlow, Action1>()
                )

                .AddDataStore(b => b
                    .AddFlow<int>("action1")
                    .AddFlow<int, Action1>()
                    .AddFlow<int, IntFlow>("action1")
                    .AddFlow<int, IntStringFlow>("action1")
                    .AddFlow<int, IntFlow, Action1>()
                    .AddFlow<int, IntStringFlow, Action1>()
                )

                .AddFork("fork1", b => b
                    .AddFlow<int>("action1")
                    .AddFlow<int, Action1>()
                    .AddFlow<int, IntFlow>("action1")
                    .AddFlow<int, IntStringFlow>("action1")
                    .AddFlow<int, IntFlow, Action1>()
                    .AddFlow<int, IntStringFlow, Action1>()
                )

                .AddFork(b => b
                    .AddFlow<int>("action1")
                    .AddFlow<int, Action1>()
                    .AddFlow<int, IntFlow>("action1")
                    .AddFlow<int, IntStringFlow>("action1")
                    .AddFlow<int, IntFlow, Action1>()
                    .AddFlow<int, IntStringFlow, Action1>()
                )

                .AddJoin("join1", b => b
                    .AddFlow<int>("action1")
                )
                .AddJoin("join1", b => b
                    .AddFlow<int, Action1>()
                )
                .AddJoin("join1", b => b
                    .AddFlow<int, IntFlow>("action1")
                )
                .AddJoin("join1", b => b
                    .AddFlow<int, IntStringFlow>("action1")
                )
                .AddJoin("join1", b => b
                    .AddFlow<int, IntFlow, Action1>()
                )
                .AddJoin("join1", b => b
                    .AddFlow<int, IntStringFlow, Action1>()
                )
                .AddJoin(b => b
                    .AddFlow<int>("action1")
                )
                .AddJoin(b => b
                    .AddFlow<int, Action1>()
                )
                .AddJoin(b => b
                    .AddFlow<int, IntFlow>("action1")
                )
                .AddJoin(b => b
                    .AddFlow<int, IntStringFlow>("action1")
                )
                .AddJoin(b => b
                    .AddFlow<int, IntFlow, Action1>()
                )
                .AddJoin(b => b
                    .AddFlow<int, IntStringFlow, Action1>()
                )
                .AddMerge("merge1", b => b
                    .AddFlow<int>("action1")
                )
                .AddMerge("merge1", b => b
                    .AddFlow<int, Action1>()
                )
                .AddMerge("merge1", b => b
                    .AddFlow<int, IntFlow>("action1")
                )
                .AddMerge("merge1", b => b
                    .AddFlow<int, IntStringFlow>("action1")
                )
                .AddMerge("merge1", b => b
                    .AddFlow<int, IntFlow, Action1>()
                )
                .AddMerge("merge1", b => b
                    .AddFlow<int, IntStringFlow, Action1>()
                )
                .AddMerge(b => b
                    .AddFlow<int>("action1")
                )
                .AddMerge(b => b
                    .AddFlow<int, Action1>()
                )
                .AddMerge(b => b
                    .AddFlow<int, IntFlow>("action1")
                )
                .AddMerge(b => b
                    .AddFlow<int, IntStringFlow>("action1")
                )
                .AddMerge(b => b
                    .AddFlow<int, IntFlow, Action1>()
                )
                .AddMerge(b => b
                    .AddFlow<int, IntStringFlow, Action1>()
                )

                .AddControlDecision("controlDecision1", b => b
                    .AddFlow("action1", b => b
                        .AddGuard(async c => true)
                    )
                    .AddFlow<Action1>(b => b
                        .AddGuard(async c => true)
                    )
                    .AddFlow<ControlFlow>("action1")
                    .AddFlow<ControlFlow, Action1>()

                    .AddElseFlow("action1", b => b
                        .SetWeight(1)
                    )
                    .AddElseFlow<Action1>(b => b
                        .SetWeight(1)
                    )
                )
                
                .AddControlDecision(b => b
                    .AddFlow("action1", b => b
                        .AddGuard(async c => true)
                    )
                    .AddFlow<Action1>(b => b
                        .AddGuard(async c => true)
                    )
                    .AddFlow<ControlFlow>("action1")
                    .AddFlow<ControlFlow, Action1>()

                    .AddElseFlow("action1", b => b
                        .SetWeight(1)
                    )
                    .AddElseFlow<Action1>(b => b
                        .SetWeight(1)
                    )
                )

                .AddDecision<int>("intDecision1", b => b
                    .AddFlow("action1", b => b
                        .AddGuard(async c => true)
                    )
                    .AddFlow<Action1>(b => b
                        .AddGuard(async c => true)
                    )
                    .AddFlow<IntFlow>("action1")
                    .AddFlow<IntFlow, Action1>()
                    .AddFlow<string, IntStringFlow>("action1")
                    .AddFlow<string, IntStringFlow, Action1>()

                    .AddElseFlow("action1", b => b
                        .SetWeight(1)
                    )
                    .AddElseFlow<Action1>(b => b
                        .SetWeight(1)
                    )
                    .AddElseFlow("action1", b => b
                        .SetWeight(1)
                        .AddTransformation<string>(async c => c.Token.ToString())
                    )
                    .AddElseFlow<Action1>(b => b
                        .SetWeight(1)
                        .AddTransformation<string>(async c => c.Token.ToString())
                    )
                    .AddElseFlow<string, IntStringFlow>("action1")
                    .AddElseFlow<string, IntStringFlow, Action1>()
                )

                .AddDecision<int>(b => b
                    .AddFlow("action1", b => b
                        .AddGuard(async c => true)
                        .AddTransformation(async c => c.Token.ToString())
                    )
                    .AddFlow<Action1>(b => b
                        .AddGuard(async c => true)
                        .AddTransformation(async c => c.Token.ToString())
                    )
                    .AddFlow<IntFlow>("action1")
                    .AddFlow<IntFlow, Action1>()
                    .AddFlow<string, IntStringFlow>("action1")
                    .AddFlow<string, IntStringFlow, Action1>()

                    .AddElseFlow("action1", b => b
                        .SetWeight(1)
                    )
                    .AddElseFlow<Action1>(b => b
                        .SetWeight(1)
                    )
                    .AddElseFlow("action1", b => b
                        .SetWeight(1)
                        .AddTransformation<string>(async c => c.Token.ToString())
                    )
                    .AddElseFlow<Action1>(b => b
                        .SetWeight(1)
                        .AddTransformation<string>(async c => c.Token.ToString())
                    )
                    .AddElseFlow<string, IntStringFlow>("action1")
                    .AddElseFlow<string, IntStringFlow, Action1>()
                )
                
                .AddAction(
                    "action1",
                    async c => { },
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                )
                
                .AddAction<Action1>(
                    "action1",
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                )

                .AddAction<Action1>(b => b
                    .AddControlFlow("action1", b => b
                        .AddGuard(async c => true)
                    )
                    .AddControlFlow<Action1>(b => b
                        .AddGuard(async c => true)
                    )
                    .AddControlFlow<ControlFlow>("action1")
                    .AddControlFlow<ControlFlow, Action1>()

                    .AddFlow<int>("action1", b => b
                        .SetWeight(1)
                        .AddGuard(async c => true)
                        .AddTransformation(async c => c.Token.ToString())
                    )
                    .AddFlow<int, Action1>(b => b
                        .SetWeight(1)
                        .AddGuard(async c => true)
                        .AddTransformation(async c => c.Token.ToString())
                    )
                    .AddFlow<int, IntFlow>("action1")
                    .AddFlow<int, IntStringFlow>("action1")
                    .AddFlow<int, IntFlow, Action1>()
                    .AddFlow<int, IntStringFlow, Action1>()

                    .AddFlow<int>("action2", b => b
                        .SetWeight(1)
                        .AddGuard(async c => true)
                        .AddTransformation(async c => c.Token.ToString())
                    )
                    .AddControlFlow("action2", b => b
                        .SetWeight(1)
                        .AddGuard(async c => true)
                    )
                )

                .AddAcceptEventAction<SomeEvent>(
                    "someAction",
                    async c => { },
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                )
                
                .AddAcceptEventAction<SomeEvent, AcceptSomeEventAction>(
                    "someAction",
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                )
                
                .AddAcceptEventAction<SomeEvent, AcceptSomeEventAction>(b => b
                    .AddControlFlow("action1", b => b
                        .AddGuard(async c => true)
                    )
                    .AddControlFlow<Action1>(b => b
                        .AddGuard(async c => true)
                    )
                    .AddControlFlow<ControlFlow>("action1")
                    .AddControlFlow<ControlFlow, Action1>()

                    .AddFlow<int>("action1", b => b
                        .SetWeight(1)
                        .AddGuard(async c => true)
                        .AddTransformation(async c => c.Token.ToString())
                    )
                    .AddFlow<int, Action1>(b => b
                        .SetWeight(1)
                        .AddGuard(async c => true)
                        .AddTransformation(async c => c.Token.ToString())
                    )
                    .AddFlow<int, IntFlow>("action1")
                    .AddFlow<int, IntStringFlow>("action1")
                    .AddFlow<int, IntFlow, Action1>()
                    .AddFlow<int, IntStringFlow, Action1>()

                    .AddFlow<int>("action2", b => b
                        .SetWeight(1)
                        .AddGuard(async c => true)
                        .AddTransformation(async c => c.Token.ToString())
                    )
                    .AddControlFlow("action2", b => b
                        .SetWeight(1)
                        .AddGuard(async c => true)
                    )
                )
                
                .AddSendEventAction(
                    "send1",
                    async c => new SomeEvent(),
                    async c => new BehaviorId("", "", ""),
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()
                )
                
                .AddTimeEventAction<AfterOneMinute>(
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                )
                
                .AddTimeEventAction<AfterOneMinute>(
                    "someAction",
                    async c => { },
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                )
                
                .AddTimeEventAction<AfterOneMinute>(
                    async c => { },
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                )
                
                .AddTimeEventAction<AfterOneMinute>(
                    "someAction",
                    b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                )
                
                .AddStructuredActivity(b => b
                    .AddInitial(b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()
                    )
                    .AddInput(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddOutput()
                    .AddFinal()
                    .AddDataStore("dataStore1", b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddDataStore(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddFork("fork1", b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddFork(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddControlDecision("controlDecision1", b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<ControlFlow>("action1")
                        .AddFlow<ControlFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                    )
                    .AddControlDecision(b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<ControlFlow>("action1")
                        .AddFlow<ControlFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                    )
                    .AddDecision<int>("intDecision1", b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<IntFlow>("action1")
                        .AddFlow<IntFlow, Action1>()
                        .AddFlow<string, IntStringFlow>("action1")
                        .AddFlow<string, IntStringFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<string, IntStringFlow>("action1")
                        .AddElseFlow<string, IntStringFlow, Action1>()
                    )
                    .AddDecision<int>(b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<IntFlow>("action1")
                        .AddFlow<IntFlow, Action1>()
                        .AddFlow<string, IntStringFlow>("action1")
                        .AddFlow<string, IntStringFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<string, IntStringFlow>("action1")
                        .AddElseFlow<string, IntStringFlow, Action1>()
                    )
                    .AddAction(
                        "action1",
                        async c => { },
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddAction<Action1>(
                        "action1",
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddAction<Action1>(b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                    )
                    .AddAcceptEventAction<SomeEvent>(
                        "someAction",
                        async c => { },
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddAcceptEventAction<SomeEvent, AcceptSomeEventAction>(
                        "someAction",
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddAcceptEventAction<SomeEvent, AcceptSomeEventAction>(b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                    )
                    .AddSendEventAction(
                        "send1",
                        async c => new SomeEvent(),
                        async c => new BehaviorId("", "", ""),
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()
                    )
                    .AddTimeEventAction<AfterOneMinute>(
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddTimeEventAction<AfterOneMinute>(
                        "someAction",
                        async c => { },
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddTimeEventAction<AfterOneMinute>(
                        async c => { },
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddTimeEventAction<AfterOneMinute>(
                        "someAction",
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                )

                .AddIterativeActivity<int>(b => b
                    .AddInitial(b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()
                    )
                    .AddInput(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddOutput()
                    .AddFinal()
                    .AddDataStore("dataStore1", b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddDataStore(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddFork("fork1", b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddFork(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddControlDecision("controlDecision1", b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<ControlFlow>("action1")
                        .AddFlow<ControlFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                    )
                    .AddControlDecision(b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<ControlFlow>("action1")
                        .AddFlow<ControlFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                    )
                    .AddDecision<int>("intDecision1", b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<IntFlow>("action1")
                        .AddFlow<IntFlow, Action1>()
                        .AddFlow<string, IntStringFlow>("action1")
                        .AddFlow<string, IntStringFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<string, IntStringFlow>("action1")
                        .AddElseFlow<string, IntStringFlow, Action1>()
                    )
                    .AddDecision<int>(b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<IntFlow>("action1")
                        .AddFlow<IntFlow, Action1>()
                        .AddFlow<string, IntStringFlow>("action1")
                        .AddFlow<string, IntStringFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<string, IntStringFlow>("action1")
                        .AddElseFlow<string, IntStringFlow, Action1>()
                    )
                    .AddAction(
                        "action1",
                        async c => { },
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddAction<Action1>(
                        "action1",
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddAction<Action1>(b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                    )
                    .AddSendEventAction(
                        "send1",
                        async c => new SomeEvent(),
                        async c => new BehaviorId("", "", ""),
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()
                    )
                )

                .AddParallelActivity<int>(b => b
                    .AddInitial(b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()
                    )
                    .AddInput(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddOutput()
                    .AddFinal()
                    .AddDataStore("dataStore1", b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddDataStore(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddFork("fork1", b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddFork(b => b
                        .AddFlow<int>("action1")
                        .AddFlow<int, Action1>()
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddJoin("join1", b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddJoin(b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddMerge("merge1", b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntFlow>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntStringFlow>("action1")
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntFlow, Action1>()
                    )
                    .AddMerge(b => b
                        .AddFlow<int, IntStringFlow, Action1>()
                    )
                    .AddControlDecision("controlDecision1", b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<ControlFlow>("action1")
                        .AddFlow<ControlFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                    )
                    .AddControlDecision(b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<ControlFlow>("action1")
                        .AddFlow<ControlFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                    )
                    .AddDecision<int>("intDecision1", b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddFlow<IntFlow>("action1")
                        .AddFlow<IntFlow, Action1>()
                        .AddFlow<string, IntStringFlow>("action1")
                        .AddFlow<string, IntStringFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<string, IntStringFlow>("action1")
                        .AddElseFlow<string, IntStringFlow, Action1>()
                    )
                    .AddDecision<int>(b => b
                        .AddFlow("action1", b => b
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<Action1>(b => b
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<IntFlow>("action1")
                        .AddFlow<IntFlow, Action1>()
                        .AddFlow<string, IntStringFlow>("action1")
                        .AddFlow<string, IntStringFlow, Action1>()

                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                        )
                        .AddElseFlow("action1", b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<Action1>(b => b
                            .SetWeight(1)
                            .AddTransformation<string>(async c => c.Token.ToString())
                        )
                        .AddElseFlow<string, IntStringFlow>("action1")
                        .AddElseFlow<string, IntStringFlow, Action1>()
                    )
                    .AddAction(
                        "action1",
                        async c => { },
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddAction<Action1>(
                        "action1",
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()

                            .AddFlow<int>("action1", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, Action1>(b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddFlow<int, IntFlow>("action1")
                            .AddFlow<int, IntStringFlow>("action1")
                            .AddFlow<int, IntFlow, Action1>()
                            .AddFlow<int, IntStringFlow, Action1>()

                            .AddFlow<int>("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                                .AddTransformation(async c => c.Token.ToString())
                            )
                            .AddControlFlow("action2", b => b
                                .SetWeight(1)
                                .AddGuard(async c => true)
                            )
                    )
                    .AddAction<Action1>(b => b
                        .AddControlFlow("action1", b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<Action1>(b => b
                            .AddGuard(async c => true)
                        )
                        .AddControlFlow<ControlFlow>("action1")
                        .AddControlFlow<ControlFlow, Action1>()

                        .AddFlow<int>("action1", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, Action1>(b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddFlow<int, IntFlow>("action1")
                        .AddFlow<int, IntStringFlow>("action1")
                        .AddFlow<int, IntFlow, Action1>()
                        .AddFlow<int, IntStringFlow, Action1>()

                        .AddFlow<int>("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                            .AddTransformation(async c => c.Token.ToString())
                        )
                        .AddControlFlow("action2", b => b
                            .SetWeight(1)
                            .AddGuard(async c => true)
                        )
                    )
                    .AddSendEventAction(
                        "send1",
                        async c => new SomeEvent(),
                        async c => new BehaviorId("", "", ""),
                        b => b
                            .AddControlFlow("action1", b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<Action1>(b => b
                                .AddGuard(async c => true)
                            )
                            .AddControlFlow<ControlFlow>("action1")
                            .AddControlFlow<ControlFlow, Action1>()
                    )
                )
                ;
        }
    }
}
