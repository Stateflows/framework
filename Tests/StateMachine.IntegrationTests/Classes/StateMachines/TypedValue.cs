using StateMachine.IntegrationTests.Tests;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    public class TypedValue : IStateMachine
    {
        public void Build(IStateMachineBuilder builder)
            => builder
                .AddInitializer<ValueInitializationRequest>(async c =>
                {
                    await c.Behavior.Values.SetAsync<string>("foo", c.InitializationEvent.Value);

                    return true;
                })
                .AddInitialState("state1", b => b
                    .AddOnEntry(async c =>
                    {
                        var (success, v) = await c.Behavior.Values.TryGetAsync<string>("foo");
                        if (success)
                        {
                            Initialization.Value = v;
                        }
                    })
                );
    }
}
