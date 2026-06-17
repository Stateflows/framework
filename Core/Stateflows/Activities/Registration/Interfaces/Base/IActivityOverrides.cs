using System.Diagnostics;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivityOverrides<out TReturn> : IActivityActionOverrides<TReturn>
    {
        #region UseStructuredActivity
        TReturn UseStructuredActivity(string actionNodeName, OverridenStructuredActivityBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn UseStructuredActivity<TStructuredActivity>(OverridenStructuredActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
            => UseStructuredActivity(ActivityNode<TStructuredActivity>.Name, buildAction);
        #endregion
        
        #region UseParallelActivity
        TReturn UseParallelActivity<TParallelizationToken>(string actionNodeName, OverridenParallelActivityBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn UseParallelActivity<TParallelizationToken, TStructuredActivity>(OverridenParallelActivityBuildAction buildAction = null)
            where TStructuredActivity : class, IStructuredActivityNode
            => UseParallelActivity<TParallelizationToken>(ActivityNode<TStructuredActivity>.Name, buildAction);
        #endregion
        
        #region UseIterativeActivity
        TReturn UseIterativeActivity<TToken>(string actionNodeName, OverridenIterativeActivityBuildAction buildAction);
        
        [DebuggerHidden]
        public TReturn UseIterativeActivity<TIterationToken, TIterativeActivity>(OverridenIterativeActivityBuildAction buildAction = null)
            where TIterativeActivity : class, IStructuredActivityNode
            => UseIterativeActivity<TIterationToken>(ActivityNode<TIterativeActivity>.Name, buildAction);
        #endregion
    }
}
