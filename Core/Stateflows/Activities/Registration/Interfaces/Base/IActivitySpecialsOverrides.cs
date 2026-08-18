using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivitySpecialsOverrides<out TReturn>
    {
        TReturn UseInitial(OverridenInitialBuildAction buildAction);
        
        TReturn UseInput(OverridenInputBuildAction buildAction);

        #region AddJoin
        [DebuggerHidden]
        public TReturn UseJoin(string joinNodeName, OverridenJoinBuildAction buildAction);

        [DebuggerHidden]
        public TReturn UseJoin(OverridenJoinBuildAction buildAction)
            => UseJoin(JoinNode.Name, buildAction);
        #endregion
        
        public TReturn UseFork(string forkNodeName, OverridenForkBuildAction buildAction);

        [DebuggerHidden]
        public TReturn UseFork(OverridenForkBuildAction buildAction)
            => UseFork(ForkNode.Name, buildAction);
        
        #region AddMerge
        [DebuggerHidden]
        public TReturn UseMerge(string mergeNodeName, OverridenMergeBuildAction buildAction);

        [DebuggerHidden]
        public TReturn UseMerge(OverridenMergeBuildAction buildAction)
            => UseMerge(MergeNode.Name, buildAction);
        #endregion
        
        #region AddControlDecision
        [DebuggerHidden]
        public TReturn UseControlDecision(string decisionNodeName, OverridenDecisionBuildAction buildAction);

        [DebuggerHidden]
        public TReturn UseControlDecision(OverridenDecisionBuildAction buildAction)
            => UseControlDecision(ControlDecisionNode.Name, buildAction);
        #endregion
        
        #region AddDecision
        [DebuggerHidden]
        public TReturn UseDecision<TToken>(string decisionNodeName, OverridenDecisionBuildAction<TToken> decisionBuildAction);

        [DebuggerHidden]
        public TReturn UseDecision<TToken>(OverridenDecisionBuildAction<TToken> buildAction)
            => UseDecision(DecisionNode<TToken>.Name, buildAction);
        #endregion
        
        #region AddDataStore
        [DebuggerHidden]
        public TReturn UseDataStore(string dataStoreNodeName, OverridenDataStoreBuildAction buildAction);

        [DebuggerHidden]
        public TReturn UseDataStore(OverridenDataStoreBuildAction buildAction)
            => UseDataStore(DataStoreNode.Name, buildAction);
        #endregion
    }
}
