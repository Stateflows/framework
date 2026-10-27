using System;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Configures structural options for an activity node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-implicit-fork-and-join" />
    public interface INodeOptions<out TReturn>
    {
        /// <summary>Replaces all options on the current node.</summary>
        /// <param name="nodeOptions">Node activation options controlling token reuse and the implicit fork/join behavior.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-implicit-fork-and-join" />
        TReturn SetOptions(NodeOptions nodeOptions);

        /// <summary>Updates node options using the current option value.</summary>
        /// <param name="nodeOptionsUpdater">Function that produces the replacement options.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-implicit-fork-and-join" />
        TReturn UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater);

        /// <summary>Disables automatic join-node insertion for this node.</summary>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-implicit-fork-and-join" />
        TReturn NoImplicitJoin()
            => UpdateOptions(options => options & ~NodeOptions.ImplicitJoin);

        /// <summary>Disables automatic fork-node insertion for this node.</summary>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-implicit-fork-and-join" />
        TReturn NoImplicitFork()
            => UpdateOptions(options => options & ~NodeOptions.ImplicitFork);
    }
}
