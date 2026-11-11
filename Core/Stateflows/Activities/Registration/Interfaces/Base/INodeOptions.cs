using System;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Configures structural options for an activity node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.1.4.-Implicit-Fork-and-Join" />
    public interface INodeOptions<out TReturn>
    {
        /// <summary>Replaces all options on the current node.</summary>
        /// <param name="nodeOptions">Node activation options controlling token reuse and the implicit fork/join behavior.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.1.4.-Implicit-Fork-and-Join" />
        TReturn SetOptions(NodeOptions nodeOptions);

        /// <summary>Updates node options using the current option value.</summary>
        /// <param name="nodeOptionsUpdater">Function that produces the replacement options.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.1.4.-Implicit-Fork-and-Join" />
        TReturn UpdateOptions(Func<NodeOptions, NodeOptions> nodeOptionsUpdater);

        /// <summary>Disables automatic join-node insertion for this node.</summary>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.1.4.-Implicit-Fork-and-Join" />
        TReturn NoImplicitJoin()
            => UpdateOptions(options => options & ~NodeOptions.ImplicitJoin);

        /// <summary>Disables automatic fork-node insertion for this node.</summary>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.1.4.-Implicit-Fork-and-Join" />
        TReturn NoImplicitFork()
            => UpdateOptions(options => options & ~NodeOptions.ImplicitFork);

        /// <summary>Allows overlapping action invocations without sharing invocation inputs or scoped services.
        /// Non-action node kinds do not support this option; it is mutually exclusive with Interleave.</summary>
        /// <returns>The current builder.</returns>
        TReturn Reentrant()
            => UpdateOptions(options => options | NodeOptions.Reentrant);
    }
}
