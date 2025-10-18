namespace Stateflows.Common
{
    public interface IStateflowsExecutionInterceptor
    {
        bool BeforeExecute(BehaviorId id, EventHolder eventHolder);
        void AfterExecute(BehaviorId id, EventHolder eventHolder);
    }
}
