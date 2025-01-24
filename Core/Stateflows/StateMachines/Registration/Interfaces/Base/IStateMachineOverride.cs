namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    public interface IStateMachineOverride<out TReturn>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TStateMachine">Base state machine to be overriden</typeparam>
        /// <param name="buildAction">Overriden state machine build action<br/>
        /// Use the following pattern to implement build action:
        /// <code>
        /// b => b
        ///     . // Use . to see available builder methods
        /// </code></param>
        TReturn UseStateMachine<TStateMachine>(OverridenStateMachineBuildAction buildAction)
            where TStateMachine : class, IStateMachine;
    }
}
