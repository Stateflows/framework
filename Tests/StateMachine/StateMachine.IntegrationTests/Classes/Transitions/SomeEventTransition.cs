namespace StateMachine.IntegrationTests.Classes.Transitions
{
    internal class SomeEventTransition : Transition<SomeEvent>
    {
        public static bool GuardFired = false;

        public static bool EffectFired = false;

        public static void Reset()
        {
            GuardFired = false;
            EffectFired = false;
        }

        public override Task<bool> GuardAsync()
        {
            GuardFired = true;
            return Task.FromResult(true);
        }

        public override Task EffectAsync()
        {
            EffectFired = true;
            return Task.CompletedTask;
        }
    }
}
