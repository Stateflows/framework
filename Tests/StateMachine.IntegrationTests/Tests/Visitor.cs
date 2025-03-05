using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using StateMachine.IntegrationTests.Classes.States;

namespace StateMachine.IntegrationTests.Tests
{
    public class SMVisitor : StateMachineVisitor
    {
        public List<Type> VertexTypes { get; set; }
        public override async Task VertexTypeAddedAsync<TVertex>(string stateMachineName, int stateMachineVersion, string vertexName)
        {
            VertexTypes.Add(typeof(TVertex));
        }
    }
    
    [TestClass]
    public class Visitor : StateflowsTestClass
    {
        private List<Type> VertexTypes = new List<Type>();
        
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
                    .AddStateMachine("visitor", b => b
                        .AddInitialState<State1>(b => b
                            .AddDefaultTransition<State2>()
                        )
                        .AddState<State2>()
                    )
                )
                ;
        }

        [TestMethod]
        public async Task TypesVisitor()
        {
            var visitor = new SMVisitor();
            visitor.VertexTypes = VertexTypes;
            var register = ServiceProvider.GetRequiredService<IStateMachinesRegister>();
            await register.VisitStateMachinesAsync(visitor);
            
            Assert.IsTrue(VertexTypes.Contains(typeof(State1)));
            Assert.IsTrue(VertexTypes.Contains(typeof(State2)));
        }
    }
}