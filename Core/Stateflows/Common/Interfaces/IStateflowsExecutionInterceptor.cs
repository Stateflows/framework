namespace Stateflows.Common
{
    public interface IStateflowsExecutionInterceptor
    {
        bool BeforeExecute(Event @event);
        void AfterExecute(Event @event);
    }
}
