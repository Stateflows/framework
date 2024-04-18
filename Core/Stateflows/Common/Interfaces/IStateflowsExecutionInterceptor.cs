namespace Stateflows.Common
{
    public interface IStateflowsExecutionInterceptor
    {
        bool BeforeExecute(object @event);
        void AfterExecute(object @event);
    }
}
