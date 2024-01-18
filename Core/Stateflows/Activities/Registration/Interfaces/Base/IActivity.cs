using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivity<out TReturn>
    {
        TReturn AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction = null);

        TReturn AddStructuredActivity(string actionNodeName, StructuredActivityBuilderAction builderAction);

        TReturn AddParallelActivity<TToken>(string actionNodeName, ParallelActivityBuilderAction builderAction)
            where TToken : Token, new();

        TReturn AddIterativeActivity<TToken>(string actionNodeName, IterativeActivityBuilderAction builderAction)
            where TToken : Token, new();
    }
}
