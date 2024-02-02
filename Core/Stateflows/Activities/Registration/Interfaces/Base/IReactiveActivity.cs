using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IReactiveActivity<out TReturn>
    {
        TReturn AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuildAction buildAction = null);

        TReturn AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuildAction buildAction);

        TReturn AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuildAction buildAction)
            where TParallelizationToken : Token, new();

        TReturn AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuildAction buildAction)
            where TIterationToken : Token, new();
    }
}
