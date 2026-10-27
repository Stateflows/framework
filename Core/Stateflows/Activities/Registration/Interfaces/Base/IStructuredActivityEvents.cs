using System;
using System.Threading.Tasks;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Provides callbacks for a structured activity's initialization and finalization lifecycle.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <remarks>Initialization callbacks run when the structured activity starts; finalization callbacks run when its
    /// lifetime ends.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
    public interface IStructuredActivityEvents<out TReturn>
    {
        /// <summary>Registers an asynchronous callback to run when the structured activity initializes.</summary>
        /// <param name="actionAsync">Asynchronous callback invoked when the node activates; it receives the node's action
        /// context.</param>
        /// <returns>The builder to continue configuring.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        TReturn AddOnInitialize(Func<IActivityNodeContext, Task> actionAsync);

        /// <summary>Registers an asynchronous callback to run when the structured activity finalizes.</summary>
        /// <param name="actionAsync">Asynchronous callback invoked when the node activates; it receives the node's action
        /// context.</param>
        /// <returns>The builder to continue configuring.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        TReturn AddOnFinalize(Func<IActivityNodeContext, Task> actionAsync);
    }
}
