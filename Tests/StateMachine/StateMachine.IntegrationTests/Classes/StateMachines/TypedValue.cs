using StateMachine.IntegrationTests.Tests;

namespace StateMachine.IntegrationTests.Classes.StateMachines
{
    public class TypedValue : StateMachine<ValueInitializationRequest>
    {
        public override async Task<bool> OnInitializeAsync(ValueInitializationRequest initializationRequest)
        {
            Context.StateMachine.Values.Set<string>("foo", initializationRequest.Value);

            return true;
        }

        public override void Build(ITypedStateMachineBuilder builder)
            => builder
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
