using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;
using StateMachine.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests
{
    public class RequestInterceptor : StateMachineInterceptor
    {
        public override bool BeforeProcessEvent<TEvent>(IEventContext<TEvent> context)
        {
            if (context.Event is Req req)
            {
                req.Respond(new Resp());
            }
            
            return true;
        }
    }

    public class Resp;

    public class Req : IRequest<Resp>;
    
    [TestClass]
    public class Request : StateflowsTestClass
    {
        private bool? StateEntered = null;

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
                    .AddStateMachine("request", b => b
                        .AddInterceptor<RequestInterceptor>()
                        .AddInitialState("state1")
                    )
                )
                ;
        }

        [TestMethod]
        public async Task RequestWithoutTransition()
        {
            StateMachineInfo? currentState = null;
            RequestResult<Resp>? result = null;

            if (StateMachineLocator.TryLocateStateMachine(new StateMachineId("request", "x"), out var sm))
            {
                await sm.SendAsync(new Initialize());
                
                result = await sm.RequestAsync(new Req());

                currentState = (await sm.GetStatusAsync()).Response;
            }

            Assert.AreEqual("state1", currentState?.CurrentStates.Value);
            Assert.AreEqual(EventStatus.NotConsumed, result?.Status);
        }
    }
}