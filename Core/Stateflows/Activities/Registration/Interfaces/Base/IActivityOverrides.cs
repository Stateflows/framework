using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Overrides structured, parallel, and iterative nodes in a base activity.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IActivityOverrides<out TReturn> : IActivityActionOverrides<TReturn>
    {
        #region UseStructuredActivity
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseStructuredActivity(string actionNodeName, OverridenStructuredActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TStructuredActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseParallelActivity<TParallelizationToken>(string actionNodeName, OverridenParallelActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseParallelActivity<TParallelizationToken, TStructuredActivity>(OverridenParallelActivityBuildAction buildAction)
            where TStructuredActivity : class, IStructuredActivityNode
            => UseParallelActivity<TParallelizationToken>(ActivityNode<TStructuredActivity>.Name, buildAction);
        #endregion
        
        #region UseIterativeActivity
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseIterativeActivity<TIterationToken>(string actionNodeName, OverridenIterativeActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseIterativeActivity<TIterationToken, TIterativeActivity>(OverridenIterativeActivityBuildAction buildAction)
            where TIterativeActivity : class, IStructuredActivityNode
            => UseIterativeActivity<TIterationToken>(ActivityNode<TIterativeActivity>.Name, buildAction);
        #endregion
    }
}
