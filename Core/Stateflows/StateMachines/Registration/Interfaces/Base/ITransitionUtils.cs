namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface ITransitionUtils<out TReturn>
    {
        /// <summary>
        /// Sets transition's IsLocal property.<br/>
        /// Difference between local and external transitions is well described at
        /// <a href="https://en.wikipedia.org/wiki/UML_state_machine#Local_versus_external_transitions">Wikipedia</a>.
        /// All transitions in Stateflows State Machines are local by default.
        /// </summary>
        /// <param name="isLocal"></param>
        TReturn SetIsLocal(bool isLocal);
    }
}
