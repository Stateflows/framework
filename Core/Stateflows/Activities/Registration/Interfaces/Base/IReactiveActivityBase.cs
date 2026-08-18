using System.Diagnostics;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces.Internal;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IReactiveActivityBase<out TReturn> : IActivityActionBase<TReturn>
    {
        #region AddStructuredActivity
        TReturn AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn AddStructuredActivity<TStructuredActivity>(ReactiveStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(ActivityNode<TStructuredActivity>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddStructuredActivity<TStructuredActivity>(string structuredActivityName, ReactiveStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
        {
            var result = AddStructuredActivity(
                structuredActivityName,
                b =>
                {
                    var builder = (StructuredActivityBuilder)b;
                    builder.AddStructuredActivityEvents<TStructuredActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TStructuredActivity));
                    buildAction?.Invoke(b);
                }
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.NodeTypeAddedAsync<TStructuredActivity>(graph.Name, graph.Version, structuredActivityName));

            return result;
        }

        [DebuggerHidden]
        public TReturn AddStructuredActivity(ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(StructuredActivityNode.Name, buildAction);
        #endregion
        
        #region AddParallelActivity
        TReturn AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize = 1);
        
        [DebuggerHidden]
        public TReturn AddParallelActivity<TParallelizationToken, TParallelActivity>(ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TParallelActivity : class, IStructuredActivityNode
            => AddParallelActivity<TParallelizationToken, TParallelActivity>(ActivityNode<TParallelActivity>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public TReturn AddParallelActivity<TParallelizationToken, TParallelActivity>(string structuredActivityName, ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TParallelActivity : class, IStructuredActivityNode
        {
            var result = AddParallelActivity<TParallelizationToken>(
                structuredActivityName,
                b =>
                {
                    var builder = (StructuredActivityBuilder)b;
                    builder.AddStructuredActivityEvents<TParallelActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TParallelActivity));
                    buildAction?.Invoke(b);
                },
                chunkSize
            );

            var graph = ((IGraphBuilder)this).Graph;
            graph.VisitingTasks.Add(visitor => visitor.NodeTypeAddedAsync<TParallelActivity>(graph.Name, graph.Version, structuredActivityName));

            return result;
        }

        [DebuggerHidden]
        public TReturn AddParallelActivity<TParallelizationToken>(ParallelActivityBuildAction buildAction, int chunkSize = 1)
            => AddParallelActivity<TParallelizationToken>(ParallelActivityNode<TParallelizationToken>.Name, buildAction, chunkSize);
        #endregion
        
        #region AddIterativeActivity
        TReturn AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize = 1);
        
        [DebuggerHidden]
        public TReturn AddIterativeActivity<TIterationToken, TIterativeActivity>(IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TIterativeActivity : class, IStructuredActivityNode
            => AddIterativeActivity<TIterationToken, TIterativeActivity>(ActivityNode<TIterativeActivity>.Name, buildAction, chunkSize);

        [DebuggerHidden]
        public TReturn AddIterativeActivity<TIterationToken, TIterativeActivity>(string structuredActivityName, IterativeActivityBuildAction buildAction = null, int chunkSize = 1)
            where TIterativeActivity : class, IStructuredActivityNode
        {
            var result = AddIterativeActivity<TIterationToken>(
                structuredActivityName,
                b =>
                {
                    var builder = (StructuredActivityBuilder)b;
                    builder.AddStructuredActivityEvents<TIterativeActivity>();
                    builder.Node.ScanForDeclaredTypes(typeof(TIterativeActivity));
                    buildAction?.Invoke(b);
                },
                chunkSize
            );

        var graph = ((IGraphBuilder)this).Graph;
        graph.VisitingTasks.Add(visitor => visitor.NodeTypeAddedAsync<TIterativeActivity>(graph.Name, graph.Version, structuredActivityName));

        return result;
    }
        
        [DebuggerHidden]
        public TReturn AddIterativeActivity<TIterationToken>(IterativeActivityBuildAction buildAction, int chunkSize = 1)
            => AddIterativeActivity<TIterationToken>(IterativeActivityNode<TIterationToken>.Name, buildAction, chunkSize);
        #endregion
    }
}
