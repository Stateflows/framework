using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Builders;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IReactiveActivity<out TReturn>
        where TReturn : class
    {
        #region AddAction
        TReturn AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuildAction buildAction = null);
        
        [DebuggerHidden]
        public TReturn AddAction<TAction>(TypedActionBuildAction buildAction = null)
            where TAction : class, IActionNode
            => AddAction<TAction>(ActivityNode<TAction>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddAction<TAction>(string actionNodeName, TypedActionBuildAction buildAction = null)
            where TAction : class, IActionNode
            => AddAction(
                actionNodeName,
                (c =>
                {
                    var action = ((BaseContext)c).NodeScope.GetAction<TAction>(c);

                    InputTokens.TokensHolder.Value = ((ActionContext)c).InputTokens;
                    OutputTokens.TokensHolder.Value = ((ActionContext)c).OutputTokens;

                    ActivityNodeContextAccessor.Context.Value = c;
                    var result = action.ExecuteAsync(c.CancellationToken);
                    ActivityNodeContextAccessor.Context.Value = null;

                    return result;
                }),
                b =>
                {
                    ((NodeBuilder)b).Node.ScanForDeclaredTypes(typeof(TAction));
                    buildAction?.Invoke(b as ITypedActionBuilder);
                }
            );
        #endregion
        
        #region AddStructuredActivity
        TReturn AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn AddStructuredActivity<TStructuredActivity>(ReactiveStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddStructuredActivity<TStructuredActivity>(ActivityNode<TStructuredActivity>.Name, buildAction);

        [DebuggerHidden]
        public TReturn AddStructuredActivity<TStructuredActivity>(string structuredActivityName, ReactiveStructuredActivityBuildAction buildAction = null)
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

        [DebuggerHidden]
        public TReturn AddStructuredActivity(ReactiveStructuredActivityBuildAction buildAction)
            => AddStructuredActivity(StructuredActivityNode.Name, buildAction);
        #endregion
        
        #region AddParallelActivity
        TReturn AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize = 1);
        
        [DebuggerHidden]
        public TReturn AddParallelActivity<TParallelizationToken, TStructuredActivity>(ParallelActivityBuildAction buildAction = null, int chunkSize = 1)
            where TStructuredActivity : class, IStructuredActivityNode
            => AddParallelActivity<TParallelizationToken, TStructuredActivity>(ActivityNode<TStructuredActivity>.Name, buildAction, chunkSize);

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
        
        [DebuggerHidden]
        public TReturn AddIterativeActivity<TIterationToken>(IterativeActivityBuildAction buildAction, int chunkSize = 1)
            => AddIterativeActivity<TIterationToken>(IterativeActivityNode<TIterationToken>.Name, buildAction, chunkSize);
        #endregion
    }
}
