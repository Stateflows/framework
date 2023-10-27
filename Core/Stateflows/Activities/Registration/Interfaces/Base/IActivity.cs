using Stateflows.Common;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivity<out TReturn>
    {
        TReturn AddAction(string actionNodeName, ActionDelegateAsync actionAsync, ActionBuilderAction buildAction = null);
     
        //#region AddSignalAction
        //TReturn AddSignalAction<TEvent>(string signalActionNodeName, SignalActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SignalActionBuilderAction signalActionBuildAction = null)
        //    where TEvent : Event, new();
        //#endregion

        //#region AddPublishAction
        //TReturn AddPublishAction<TEvent>(string signalActionNodeName, SignalActionDelegateAsync<TEvent> actionAsync, BehaviorIdSelectorAsync targetSelectorAsync, SignalActionBuilderAction signalActionBuildAction = null)
        //    where TEvent : Event, new();
        //#endregion

        //#region AddEventAction
        //TReturn AddEventAction<TEvent>(string actionNodeName, EventActionDelegateAsync<TEvent> eventActionAsync, EventActionBuilderAction buildAction = null)
        //    where TEvent : Event, new();

        ////TReturn AddEventAction<TEvent, TEventAction>(EventActionBuilderAction buildAction = null)
        ////    where TEvent : Event
        ////    where TEventAction : EventAction<TEvent>;

        ////TReturn AddEventAction<TEvent, TEventAction>(string actionNodeName, EventActionBuilderAction buildAction = null)
        ////    where TEvent : Event
        ////    where TEventAction : EventAction<TEvent>;
        //#endregion

        TReturn AddStructuredActivity(string actionNodeName, StructuredActivityBuilderAction builderAction = null);

        TReturn AddParallelActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction = null)
            where TToken : Token, new();

        TReturn AddIterativeActivity<TToken>(string actionNodeName, StructuredActivityBuilderAction builderAction = null)
            where TToken : Token, new();
    }
}
