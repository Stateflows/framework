using System.Collections.Generic;
using System.Diagnostics;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Common;
using Stateflows.Payloads;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Overrides reactive structured, parallel, and iterative nodes in a base activity.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
    public interface IReactiveActivityOverrides<out TReturn> : IActivityActionOverrides<TReturn>
    {
        #region UseStructuredActivity
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseStructuredActivity(string actionNodeName, OverridenReactiveStructuredActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseStructuredActivity(OverridenReactiveStructuredActivityBuildAction buildAction)
            => UseStructuredActivity(StructuredActivityNode.Name, buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TStructuredActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseStructuredActivity<TStructuredActivity>(OverridenReactiveStructuredActivityBuildAction buildAction)
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseParallelActivity<TParallelizationToken>(string actionNodeName, OverridenParallelActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseParallelActivity<TParallelizationToken>(OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(ParallelActivityNode<TParallelizationToken>.Name, buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <typeparam name="TParallelActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseParallelActivity<TParallelizationToken, TParallelActivity>(OverridenParallelActivityBuildAction buildAction)
            where TParallelActivity : class, IStructuredActivityNode
            => UseParallelActivity<TParallelizationToken>(ActivityNode<TParallelActivity>.Name, buildAction);

        /// <summary>Overrides a parallel activity for payload tokens with the specified names.</summary>
        public TReturn UseParallelActivity(IReadOnlyList<string> parallelizationTokenNames, string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => PayloadFlowScope.With(parallelizationTokenNames, nameof(parallelizationTokenNames),
                () => UseParallelActivity<Payload>(actionNodeName, buildAction));

        /// <summary>Overrides the default-named parallel activity for payload tokens with the specified names.</summary>
        public TReturn UseParallelActivity(IReadOnlyList<string> parallelizationTokenNames,
            OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity(parallelizationTokenNames, ParallelActivityNode.GetName(parallelizationTokenNames), buildAction);

        /// <summary>Overrides a parallel activity for payload tokens with the specified name.</summary>
        public TReturn UseParallelActivity(string parallelizationTokenName, string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity(new[] { parallelizationTokenName }, actionNodeName, buildAction);

        /// <summary>Overrides a typed parallel activity implementation for the specified payload token names.</summary>
        public TReturn UseParallelActivity<TParallelActivity>(IReadOnlyList<string> parallelizationTokenNames,
            OverridenParallelActivityBuildAction buildAction)
            where TParallelActivity : class, IStructuredActivityNode
            => PayloadFlowScope.With(parallelizationTokenNames, nameof(parallelizationTokenNames),
                () => UseParallelActivity<Payload, TParallelActivity>(buildAction));

        /// <summary>Overrides a typed parallel activity implementation for the specified payload token names.</summary>
        public TReturn UseParallelActivity<TParallelActivity>(IReadOnlyList<string> parallelizationTokenNames,
            string actionNodeName, OverridenParallelActivityBuildAction buildAction)
            where TParallelActivity : class, IStructuredActivityNode
            => UseParallelActivity(parallelizationTokenNames, actionNodeName, buildAction);

        /// <summary>Overrides a typed parallel activity implementation for the specified payload token name.</summary>
        public TReturn UseParallelActivity<TParallelActivity>(string parallelizationTokenName, string actionNodeName,
            OverridenParallelActivityBuildAction buildAction)
            where TParallelActivity : class, IStructuredActivityNode
            => UseParallelActivity<TParallelActivity>(new[] { parallelizationTokenName }, actionNodeName, buildAction);
        #endregion
        
        #region UseIterativeActivity
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        TReturn UseIterativeActivity<TToken>(string actionNodeName, OverridenIterativeActivityBuildAction buildAction);
        
        /// <summary>Selects and extends the inherited Activity definition.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
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
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.5.2.1.15.-Structured-Activity-Node" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/5.-Behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseIterativeActivity<TIterationToken, TIterativeActivity>(OverridenIterativeActivityBuildAction buildAction)
            where TIterativeActivity : class, IStructuredActivityNode
            => UseIterativeActivity<TIterationToken>(ActivityNode<TIterativeActivity>.Name, buildAction);

        /// <summary>Overrides an iterative activity for payload tokens with the specified names.</summary>
        public TReturn UseIterativeActivity(IReadOnlyList<string> iterationTokenNames, string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => PayloadFlowScope.With(iterationTokenNames, nameof(iterationTokenNames),
                () => UseIterativeActivity<Payload>(actionNodeName, buildAction));

        /// <summary>Overrides the default-named iterative activity for payload tokens with the specified names.</summary>
        public TReturn UseIterativeActivity(IReadOnlyList<string> iterationTokenNames,
            OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity(iterationTokenNames, IterativeActivityNode.GetName(iterationTokenNames), buildAction);

        /// <summary>Overrides an iterative activity for payload tokens with the specified name.</summary>
        public TReturn UseIterativeActivity(string iterationTokenName, string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity(new[] { iterationTokenName }, actionNodeName, buildAction);

        /// <summary>Overrides a typed iterative activity implementation for the specified payload token names.</summary>
        public TReturn UseIterativeActivity<TIterativeActivity>(IReadOnlyList<string> iterationTokenNames,
            OverridenIterativeActivityBuildAction buildAction)
            where TIterativeActivity : class, IStructuredActivityNode
            => PayloadFlowScope.With(iterationTokenNames, nameof(iterationTokenNames),
                () => UseIterativeActivity<Payload, TIterativeActivity>(buildAction));

        /// <summary>Overrides a typed iterative activity implementation for the specified payload token names.</summary>
        public TReturn UseIterativeActivity<TIterativeActivity>(IReadOnlyList<string> iterationTokenNames,
            string actionNodeName, OverridenIterativeActivityBuildAction buildAction)
            where TIterativeActivity : class, IStructuredActivityNode
            => UseIterativeActivity(iterationTokenNames, actionNodeName, buildAction);

        /// <summary>Overrides a typed iterative activity implementation for the specified payload token name.</summary>
        public TReturn UseIterativeActivity<TIterativeActivity>(string iterationTokenName, string actionNodeName,
            OverridenIterativeActivityBuildAction buildAction)
            where TIterativeActivity : class, IStructuredActivityNode
            => UseIterativeActivity<TIterativeActivity>(new[] { iterationTokenName }, actionNodeName, buildAction);
        #endregion
    }
}
