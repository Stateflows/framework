using System.Diagnostics;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Extensions;
using Stateflows.Activities.Registration.Builders;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivitySpecials<out TReturn>
        where TReturn : class
    {
        #region AddJoin
        [DebuggerHidden]
        public TReturn AddJoin(string joinNodeName, JoinBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Join,
                    joinNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b)
                ) as TReturn;

        [DebuggerHidden]
        public TReturn AddJoin(JoinBuildAction buildAction)
            => AddJoin(JoinNode.Name, buildAction);
        #endregion
        
        #region AddFork
        [DebuggerHidden]
        public TReturn AddFork(string forkNodeName, ForkBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Fork,
                    forkNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b)
                ) as TReturn;

        [DebuggerHidden]
        public TReturn AddFork(ForkBuildAction buildAction)
            => AddFork(ForkNode.Name, buildAction);
        #endregion
        
        #region AddMerge
        [DebuggerHidden]
        public TReturn AddMerge(string mergeNodeName, MergeBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Merge,
                    mergeNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.None) as IMergeBuilder)
                ) as TReturn;

        [DebuggerHidden]
        public TReturn AddMerge(MergeBuildAction buildAction)
            => AddMerge(MergeNode.Name, buildAction);
        #endregion
        
        #region AddControlDecision
        [DebuggerHidden]
        public TReturn AddControlDecision(string decisionNodeName, DecisionBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.DecisionDefault) as IDecisionBuilder)
                ) as TReturn;

        [DebuggerHidden]
        public TReturn AddControlDecision(DecisionBuildAction buildAction)
            => AddControlDecision(ControlDecisionNode.Name, buildAction);
        #endregion
        
        #region AddDecision
        [DebuggerHidden]
        public TReturn AddDecision<TToken>(string decisionNodeName, DecisionBuildAction<TToken> decisionBuildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.Decision,
                    decisionNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => decisionBuildAction(new DecisionBuilder<TToken>(b.SetOptions(NodeOptions.DecisionDefault) as NodeBuilder))
                ) as TReturn;

        [DebuggerHidden]
        public TReturn AddDecision<TToken>(DecisionBuildAction<TToken> buildAction)
            => AddDecision(DecisionNode<TToken>.Name, buildAction);
        #endregion
        
        #region AddDataStore
        [DebuggerHidden]
        public TReturn AddDataStore(string dataStoreNodeName, DataStoreBuildAction buildAction)
            => ((BaseActivityBuilder)this)
                .AddNode(
                    NodeType.DataStore,
                    dataStoreNodeName,
                    c =>
                    {
                        c.PassAllTokensOn();
                        return Task.CompletedTask;
                    },
                    b => buildAction(b.SetOptions(NodeOptions.DataStoreDefault) as IDataStoreBuilder)
                ) as TReturn;

        [DebuggerHidden]
        public TReturn AddDataStore(DataStoreBuildAction buildAction)
            => AddDataStore(DataStoreNode.Name, buildAction);
        #endregion
    }
}
