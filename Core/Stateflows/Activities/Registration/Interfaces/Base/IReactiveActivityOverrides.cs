using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IReactiveActivityOverrides<out TReturn> : IActivityActionOverrides<TReturn>
    {
        #region UseStructuredActivity
        TReturn UseStructuredActivity(string actionNodeName, OverridenReactiveStructuredActivityBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn UseStructuredActivity(OverridenReactiveStructuredActivityBuildAction buildAction)
            => UseStructuredActivity(StructuredActivityNode.Name, buildAction);
        
        [DebuggerHidden]
        public TReturn UseStructuredActivity<TStructuredActivity>(OverridenReactiveStructuredActivityBuildAction buildAction)
            where TStructuredActivity : class, IStructuredActivityNode
            => UseStructuredActivity(ActivityNode<TStructuredActivity>.Name, buildAction);
        #endregion
        
        #region UseParallelActivity
        TReturn UseParallelActivity<TParallelizationToken>(string actionNodeName, OverridenParallelActivityBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn UseParallelActivity<TParallelizationToken>(OverridenParallelActivityBuildAction buildAction)
            => UseParallelActivity<TParallelizationToken>(ParallelActivityNode<TParallelizationToken>.Name, buildAction);
        
        [DebuggerHidden]
        public TReturn UseParallelActivity<TParallelizationToken, TParallelActivity>(OverridenParallelActivityBuildAction buildAction)
            where TParallelActivity : class, IStructuredActivityNode
            => UseParallelActivity<TParallelizationToken>(ActivityNode<TParallelActivity>.Name, buildAction);
        #endregion
        
        #region UseIterativeActivity
        TReturn UseIterativeActivity<TToken>(string actionNodeName, OverridenIterativeActivityBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn UseIterativeActivity<TIterationToken>(OverridenIterativeActivityBuildAction buildAction)
            => UseIterativeActivity<TIterationToken>(IterativeActivityNode<TIterationToken>.Name, buildAction);
        
        [DebuggerHidden]
        public TReturn UseIterativeActivity<TIterationToken, TIterativeActivity>(OverridenIterativeActivityBuildAction buildAction)
            where TIterativeActivity : class, IStructuredActivityNode
            => UseIterativeActivity<TIterationToken>(ActivityNode<TIterativeActivity>.Name, buildAction);
        #endregion
    }
}
