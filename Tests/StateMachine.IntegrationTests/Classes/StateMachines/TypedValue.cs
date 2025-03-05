using StateMachine.IntegrationTests.Tests;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    public class TypedValue : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
            => builder
                .AddInitializer<ValueInitializationRequest>(async c =>
                {
                    c.Behavior.Values.Set<string>("foo", c.InitializationEvent.Value);

                    return true;
                })
                .AddInitialState("state1", b => b
                    .AddOnEntry(c =>
                    {
                        if (c.Behavior.Values.TryGet<string>("foo", out var v))
                        {
                            Initialization.Value = v;
                        }
                    })
                );
    }
}
