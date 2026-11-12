namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Configures whether a targeted transition preserves an enclosing active state when traversing its
    /// hierarchy.</summary>
    /// <typeparam name="TReturn">The transition builder returned after configuration.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.2.4.1.-Transition" />
    public interface ITargetedTransitionUtils<out TReturn>
    {
        /// <summary>Selects local or external exit and re-entry semantics for the current targeted transition.</summary>
        /// <param name="isLocal">True to retain the common enclosing state where local-transition rules permit; false to
        /// use external exit and re-entry semantics.</param>
        /// <returns>The current transition builder.</returns>
        /// <remarks>Transitions are local by default. Local transitions preserve the common enclosing active state where
        /// the hierarchy permits; external transitions exit and re-enter it.</remarks>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.4.1.3.-Evaluation-of-Transitions#local-transitions" />
        TReturn SetIsLocal(bool isLocal);
    }
}
