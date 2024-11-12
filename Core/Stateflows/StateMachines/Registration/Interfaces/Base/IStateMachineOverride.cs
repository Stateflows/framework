namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineOverride<out TReturn>
    {
        TReturn UseStateMachine<TStateMachine>(OverridenStateMachineBuildAction buildAction)
            where TStateMachine : class, IStateMachine;
    }
}
