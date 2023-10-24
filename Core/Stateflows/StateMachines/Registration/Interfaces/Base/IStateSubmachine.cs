namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateSubmachine<out TReturn>
    {
        TReturn AddSubmachine(string submachineName, StateActionInitializationBuilder initializationBuilder = null);
    }
}
