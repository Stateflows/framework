using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    public class BaseStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder) => builder
            .AddInitialState("initial", b => b
                .AddTransition<SomeEvent>("state1")
            )
            .AddState("state1")
            ;
    }

    public class InheritedStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder) => builder
            .UseStateMachine<BaseStateMachine>(b => b
                .UseState("state1", b => b
                    .AddDefaultTransition("state2")
                )
                .AddState("state2")
            );
    }

    public class OrthogonalizedStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder) => builder
            .UseStateMachine<BaseStateMachine>(b => b
                .UseState("state1", b => b
                    .MakeOrthogonal(b => b
                        .AddRegion(b => b
                            .AddInitialState("state1.A.1")
                        )
                        .AddRegion(b => b
                            .AddInitialState("state1.B.1")
                        )
                    )
                )
            );
    }

    public class InheritedOrthogonalStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder) => builder
            .UseStateMachine<OrthogonalizedStateMachine>(b => b
                .UseOrthogonalState("state1", b => b
                    .UseRegion(0, b => b
                        .UseState("state1.A.1", b => b
                            .AddDefaultTransition("state1.A.2")
                        )
                        .AddState("state1.A.2")
                    )
                    .UseRegion(1, b => b
                        .UseState("state1.B.1", b => b
                            .AddDefaultTransition("state1.B.2")
                        )
                        .AddState("state1.B.2")
                    )
                )
            );
    }

    public class OverrideState1 : IState { }
    public class OverrideState2 : IState { }
    public class OverrideState3 : IState { }
    
    public class BaseTypedStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder) => builder
            .AddInitialState<OverrideState1>(b => b
                .AddTransition<SomeEvent, OverrideState2>()
            )
            .AddState<OverrideState2>()
        ;
    }

    public class CompositeStateMachine : IStateMachine
    {
        public void Build(IStateMachineBuilder builder) => builder
            .UseStateMachine<BaseStateMachine>(b => b
                .UseState("state1", b => b
                    .AddDefaultTransition("state2")
                )
                .AddCompositeState("state2", b => b
                    .AddInitialState("compositeInitial")
                )
            );
    }
    
    [TestClass]
    public class Override : StateflowsTestClass
    {
        public bool Entered = false;
        
        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddStateMachines(b => b

                    .AddStateMachine("extend", b => b
                        .UseStateMachine<BaseStateMachine>(b => b
                            .UseState("state1", b => b
                                .AddDefaultTransition("state2")
                            )
                            .AddState("state2")
                        )
                    )
                    
                    .AddStateMachine("cascade", b => b
                        .UseStateMachine<InheritedStateMachine>(b => b
                            .UseState("state2", b => b
                                .AddDefaultTransition("state3")
                            )
                            .AddState("state3")
                        )
                    )
                    
                    .AddStateMachine("transitionExtend", b => b
                        .UseStateMachine<BaseStateMachine>(b => b
                            .UseState("initial", b => b
                                .UseTransition<SomeEvent>("state1", b => b
                                    .AddGuard(async c => false)
                                )
                            )
                        )
                    )
                    
                    .AddStateMachine("stateExtend", b => b
                        .UseStateMachine<BaseStateMachine>(b => b
                            .UseState("state1", b => b
                                .AddOnEntry(async c => Entered = true)
                            )
                        )
                    )
                    
                    .AddStateMachine("composite", b => b
                        .UseStateMachine<BaseStateMachine>(b => b
                            .UseState("state1", b => b
                                .MakeComposite(b => b
                                    .AddInitialState("composited")
                                )
                            )
                        )
                    )
                
                    .AddStateMachine("compositeExtend", b => b
                        .UseStateMachine<CompositeStateMachine>(b => b
                            .UseCompositeState("state2", b => b
                                .UseState("compositeInitial", b => b
                                    .AddDefaultTransition("state3")
                                )
                                .AddState("state3")
                            )
                        )
                    )
                
                    .AddStateMachine("typed", b => b
                        .UseStateMachine<BaseTypedStateMachine>(b => b
                            .UseState<OverrideState2>(b => b
                                .AddDefaultTransition<OverrideState3>()
                            )
                            .AddState<OverrideState3>()
                        )
                    )

                    .AddStateMachine<OrthogonalizedStateMachine>(nameof(OrthogonalizedStateMachine))

                    .AddStateMachine<InheritedOrthogonalStateMachine>(nameof(InheritedOrthogonalStateMachine))
                )
                ;
        }

        [TestMethod]
        public async Task AddedState()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("extend", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state2", currentState);
        }

        [TestMethod]
        public async Task CascadeExtension()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("cascade", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task ExtendedTransition()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("transitionExtend", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.AreEqual(EventStatus.NotConsumed, status);
            Assert.AreEqual("initial", currentState);
        }

        [TestMethod]
        public async Task ExtendedState()
        {
            var status = EventStatus.Rejected;
            string currentState = "initial";
            Entered = false;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("stateExtend", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.Value;
            }

            Assert.IsTrue(Entered);
            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1", currentState);
        }

        [TestMethod]
        public async Task CompositedState()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("composite", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.AllNodes_ChildrenFirst.First().Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("composited", currentState);
        }

        [TestMethod]
        public async Task ExtendedCompositeState()
        {
            var status = EventStatus.Rejected;
            string currentState = "state1";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("compositeExtend", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.AllNodes_ChildrenFirst.First().Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state3", currentState);
        }

        [TestMethod]
        public async Task Typed()
        {
            var status = EventStatus.Rejected;
            string currentState = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("typed", "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                currentState = (await sm.GetCurrentStateAsync()).Response.StatesTree.AllNodes_ChildrenFirst.First().Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual(State<OverrideState3>.Name, currentState);
        }

        [TestMethod]
        public async Task OrthogonalizedState()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string currentSubstate1 = "";
            string currentSubstate2 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId(nameof(OrthogonalizedStateMachine), "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                var tree = (await sm.GetCurrentStateAsync()).Response.StatesTree;
                currentState = tree.Value;
                currentSubstate1 = tree.Root.Nodes.First().Value;
                currentSubstate2 = tree.Root.Nodes.Last().Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1", currentState);
            Assert.AreEqual("state1.A.1", currentSubstate1);
            Assert.AreEqual("state1.B.1", currentSubstate2);
        }

        [TestMethod]
        public async Task InheritedOrthogonalState()
        {
            var status = EventStatus.Rejected;
            string currentState = "";
            string currentSubstate1 = "";
            string currentSubstate2 = "";

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId(nameof(InheritedOrthogonalStateMachine), "x"), out var sm))
            {
                status = (await sm.SendAsync(new SomeEvent())).Status;

                var tree = (await sm.GetCurrentStateAsync()).Response.StatesTree;
                currentState = tree.Value;
                currentSubstate1 = tree.Root.Nodes.First().Value;
                currentSubstate2 = tree.Root.Nodes.Last().Value;
            }

            Assert.AreEqual(EventStatus.Consumed, status);
            Assert.AreEqual("state1", currentState);
            Assert.AreEqual("state1.A.2", currentSubstate1);
            Assert.AreEqual("state1.B.2", currentSubstate2);
        }
    }
}