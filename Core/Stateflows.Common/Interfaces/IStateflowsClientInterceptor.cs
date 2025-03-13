namespace Stateflows.Common
{
    public interface IStateflowsClientInterceptor
    {
        bool BeforeDispatchEvent(EventHolder eventHolder);
        void AfterDispatchEvent(EventHolder eventHolder);
    }
}
