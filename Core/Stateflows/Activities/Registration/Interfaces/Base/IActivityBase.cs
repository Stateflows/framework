using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Builders;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds nested, parallel, and iterative activity nodes.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <remarks>Nested activities execute a child graph; parallel and iterative nodes split token work into chunks.
    /// </remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
    public interface IActivityBase<out TReturn> : IActivityActionBase<TReturn>
    {
        #region AddStructuredActivity
        /// <summary>Adds a nested Activity node with its own graph and boundary nodes.</summary>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the nested Activity's graph. Its builder is scoped to the node or
        /// flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        TReturn AddStructuredActivity(string actionNodeName, StructuredActivityBuildAction buildAction);
        
        /// <summary>Adds a nested Activity node with its own graph and boundary nodes.</summary>
        /// <typeparam name="TStructuredActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddStructuredActivity<TStructuredActivity>(StructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(ActivityNode<TStructuredActivity>.Name, buildAction);

        /// <summary>Adds a nested Activity node with its own graph and boundary nodes.</summary>
        /// <typeparam name="TStructuredActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="structuredActivityName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddStructuredActivity<TStructuredActivity>(string structuredActivityName, StructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddStructuredActivity(
                structuredActivityName,
                b =>
                {
                    var builder = (StructuredActivityBuilder)b;
                    builder.AddStructuredActivityEvents<TStructuredActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                }
            );

        /// <summary>Adds a nested Activity node with its own graph and boundary nodes.</summary>
        /// <param name="buildAction">Callback configuring the nested Activity's graph. Its builder is scoped to the node or
        /// flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddStructuredActivity(StructuredActivityBuildAction buildAction)
            => AddStructuredActivity(StructuredActivityNode.Name, buildAction);
        #endregion
        
        #region AddParallelActivity
        /// <summary>Adds a nested Activity node that processes token work items in chunks.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the Activity graph applied to each token chunk. Its builder is
        /// scoped to the node or flow, not the enclosing Activity.</param>
        /// <param name="chunkSize">Maximum number of parallelization tokens processed in one chunk.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        TReturn AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize = 1);
        
        /// <summary>Adds a nested Activity node that processes token work items in chunks.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <typeparam name="TStructuredActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <param name="chunkSize">Maximum number of parallelization tokens processed in one chunk.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddParallelActivity<TParallelizationToken, TStructuredActivity>(ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddParallelActivity<TParallelizationToken, TStructuredActivity>(ActivityNode<TStructuredActivity>.Name, buildAction, chunkSize);

        /// <summary>Adds a nested Activity node that processes token work items in chunks.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <typeparam name="TStructuredActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="structuredActivityName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <param name="chunkSize">Maximum number of parallelization tokens processed in one chunk.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddParallelActivity<TParallelizationToken, TStructuredActivity>(string structuredActivityName, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddParallelActivity<TParallelizationToken>(
                structuredActivityName,
                b =>
                {
                    var builder = b as StructuredActivityBuilder;
                    builder.AddStructuredActivityEvents<TStructuredActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                },
                chunkSize
            );

        /// <summary>Adds a nested Activity node that processes token work items in chunks.</summary>
        /// <typeparam name="TParallelizationToken">Token type identifying each unit of parallel work.</typeparam>
        /// <param name="buildAction">Callback configuring the Activity graph applied to each token chunk. Its builder is
        /// scoped to the node or flow, not the enclosing Activity.</param>
        /// <param name="chunkSize">Maximum number of parallelization tokens processed in one chunk.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddParallelActivity<TParallelizationToken>(ParallelActivityBuildAction buildAction, int chunkSize = 1)
            => AddParallelActivity<TParallelizationToken>(ParallelActivityNode<TParallelizationToken>.Name, buildAction, chunkSize);
        #endregion
        
        #region AddIterativeActivity
        /// <summary>Adds a nested Activity node that repeatedly processes token work items in chunks.</summary>
        /// <typeparam name="TToken">Token type carried by the object flow.</typeparam>
        /// <param name="actionNodeName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the Activity graph applied to each iteration chunk. Its builder
        /// is scoped to the node or flow, not the enclosing Activity.</param>
        /// <param name="chunkSize">Maximum number of iteration tokens processed in one chunk.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        TReturn AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize = 1);
        
        /// <summary>Adds a nested Activity node that repeatedly processes token work items in chunks.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <typeparam name="TIterativeActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <param name="chunkSize">Maximum number of iteration tokens processed in one chunk.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddIterativeActivity<TIterationToken, TIterativeActivity>(IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TIterativeActivity : class, IStructuredActivityNode
            => AddIterativeActivity<TIterationToken, TIterativeActivity>(ActivityNode<TIterativeActivity>.Name, buildAction, chunkSize);

        /// <summary>Adds a nested Activity node that repeatedly processes token work items in chunks.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <typeparam name="TIterativeActivity">Typed node implementation implementing
        /// <see cref="IStructuredActivityNode" />.</typeparam>
        /// <param name="structuredActivityName">Registered name of this node in its containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <param name="chunkSize">Maximum number of iteration tokens processed in one chunk.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddIterativeActivity<TIterationToken, TIterativeActivity>(string structuredActivityName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TIterativeActivity : class, IStructuredActivityNode
            => AddIterativeActivity<TIterationToken>(
                structuredActivityName,
                b =>
                {
                    var builder = b as StructuredActivityBuilder;
                    builder.AddStructuredActivityEvents<TIterativeActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TIterativeActivity));
                    buildAction?.Invoke(b);
                },
                chunkSize
            );
        
        /// <summary>Adds a nested Activity node that repeatedly processes token work items in chunks.</summary>
        /// <typeparam name="TIterationToken">Token type processed by each iteration.</typeparam>
        /// <param name="buildAction">Callback configuring the Activity graph applied to each iteration chunk. Its builder
        /// is scoped to the node or flow, not the enclosing Activity.</param>
        /// <param name="chunkSize">Maximum number of iteration tokens processed in one chunk.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-structured-activity-node" />
        [DebuggerHidden]
        public TReturn AddIterativeActivity<TIterationToken>(IterativeActivityBuildAction buildAction, int chunkSize = 1)
            => AddIterativeActivity<TIterationToken>(IterativeActivityNode<TIterationToken>.Name, buildAction, chunkSize);
        #endregion
    }
}
