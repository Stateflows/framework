using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Overrides structured, parallel, and iterative nodes in a base activity.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
    public interface IActivityOverrides<out TReturn> : IActivityActionOverrides<TReturn>
    {
        #region UseStructuredActivity
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseStructuredActivity(string actionNodeName, OverridenStructuredActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TStructuredActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseStructuredActivity<TStructuredActivity>(OverridenStructuredActivityBuildAction buildAction)
            where TStructuredActivity : class, IStructuredActivityNode
            => UseStructuredActivity(ActivityNode<TStructuredActivity>.Name, buildAction);
        #endregion
        
        #region UseParallelActivity
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseParallelActivity<TParallelizationToken>(string actionNodeName, OverridenParallelActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseParallelActivity<TParallelizationToken>(OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(ParallelActivityNode<TParallelizationToken>.Name, buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <typeparam name="TStructuredActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseParallelActivity<TParallelizationToken, TStructuredActivity>(OverridenParallelActivityBuildAction buildAction)
            where TStructuredActivity : class, IStructuredActivityNode
            => UseParallelActivity<TParallelizationToken>(ActivityNode<TStructuredActivity>.Name, buildAction);

        /// <summary>Overrides a parallel activity for payload tokens with the specified names.</summary>
        public TReturn UsePayloadParallelActivity(IReadOnlyList<string> parallelizationTokenNames, string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => PayloadFlowScope.With(parallelizationTokenNames, nameof(parallelizationTokenNames),
                () => UseParallelActivity<Payload>(actionNodeName, buildAction));

        /// <summary>Overrides the default-named parallel activity for payload tokens with the specified names.</summary>
        public TReturn UsePayloadParallelActivity(IReadOnlyList<string> parallelizationTokenNames,
            OverridenParallelActivityBuildAction buildAction)
            => UsePayloadParallelActivity(parallelizationTokenNames, ParallelActivityNode.GetName(parallelizationTokenNames), buildAction);
        #endregion
        
        #region UseIterativeActivity
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseIterativeActivity<TIterationToken>(string actionNodeName, OverridenIterativeActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseIterativeActivity<TIterationToken>(OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity<TIterationToken>(IterativeActivityNode<TIterationToken>.Name, buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <typeparam name="TIterativeActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseIterativeActivity<TIterationToken, TIterativeActivity>(OverridenIterativeActivityBuildAction buildAction)
            where TIterativeActivity : class, IStructuredActivityNode
            => UseIterativeActivity<TIterationToken>(ActivityNode<TIterativeActivity>.Name, buildAction);

        /// <summary>Overrides an iterative activity for payload tokens with the specified names.</summary>
        public TReturn UsePayloadIterativeActivity(IReadOnlyList<string> iterationTokenNames, string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => PayloadFlowScope.With(iterationTokenNames, nameof(iterationTokenNames),
                () => UseIterativeActivity<Payload>(actionNodeName, buildAction));

        /// <summary>Overrides the default-named iterative activity for payload tokens with the specified names.</summary>
        public TReturn UsePayloadIterativeActivity(IReadOnlyList<string> iterationTokenNames,
            OverridenIterativeActivityBuildAction buildAction)
            => UsePayloadIterativeActivity(iterationTokenNames, IterativeActivityNode.GetName(iterationTokenNames), buildAction);
        #endregion
    }
}
