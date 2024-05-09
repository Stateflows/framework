namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IReactiveActivity<out TReturn>
    {
        TReturn AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuildAction buildAction = null);

        TReturn AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction);

        TReturn AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuildAction buildAction, int chunkSize = 1);

        TReturn AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction, int chunkSize = 1);
    }
}
