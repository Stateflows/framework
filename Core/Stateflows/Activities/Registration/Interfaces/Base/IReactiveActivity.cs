using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IReactiveActivity<out TReturn>
    {
        TReturn AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction = null);

        TReturn AddStructuredActivity(string actionNodeName, ReactiveStructuredActivityBuilderAction builderAction);

        TReturn AddParallelActivity<TParallelizationToken>(string actionNodeName, ParallelActivityBuilderAction builderAction)
            where TParallelizationToken : Token, new();

        TReturn AddIterativeActivity<TIterationToken>(string actionNodeName, IterativeActivityBuilderAction builderAction)
            where TIterationToken : Token, new();
    }
}
