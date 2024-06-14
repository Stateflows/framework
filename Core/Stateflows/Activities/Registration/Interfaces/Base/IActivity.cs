using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivity<out TReturn>
    {
        TReturn AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuildAction buildAction = null);

        TReturn AddStructuredActivity(string actionNodeName, StructuredActivityBuildAction buildAction);

        TReturn AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuildAction buildAction);

        TReturn AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuildAction buildAction);
    }
}
