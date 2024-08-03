using StateMachine.IntegrationTests.Tests;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    public class TypedValue : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
            => builder
                .AddInitializer<ValueInitializationRequest>(c =>
                {
                    c.StateMachine.Values.Set<string>("foo", c.InitializationEvent.Value);

                    return true;
                })
                .AddInitialState("state1", b => b
                    .AddOnEntry(c =>
                    {
                        if (c.StateMachine.Values.TryGet<string>("foo", out var v))
                        {
                            Initialization.Value = v;
                        }
                    })
                );
    }
}
