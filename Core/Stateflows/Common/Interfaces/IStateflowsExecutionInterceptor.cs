namespace Stateflows.Common
{
    public interface IStateflowsExecutionInterceptor
    {
        bool BeforeExecute(EventHolder eventHolder);
        void AfterExecute(EventHolder eventHolder);
    }
}
