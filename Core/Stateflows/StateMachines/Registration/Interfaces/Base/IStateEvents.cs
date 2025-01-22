namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateEvents<out TReturn> :
        IStateEntry<TReturn>,
        IStateExit<TReturn>
    { }
}
