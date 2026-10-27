namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Combines entry and exit callback registration for the current state.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/state-machine-state" />
    public interface IStateEvents<out TReturn> :
        IStateEntry<TReturn>,
        IStateExit<TReturn>;
}
