namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Combines entry and exit callback registration for the current state.</summary>
    /// <typeparam name="TReturn">The fluent state builder returned after registration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.2.1.-State" />
    public interface IStateEvents<out TReturn> :
        IStateEntry<TReturn>,
        IStateExit<TReturn>;
}
