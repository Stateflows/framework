namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateEvents<TReturn> :
        IStateEntry<TReturn>,
        IStateExit<TReturn>
    { }
}
