namespace Stateflows.Common
{
    public interface IExecutionInterceptor
    {
        bool BeforeExecute(Event @event = null);
        void AfterExecute(Event @event = null);
    }
}
