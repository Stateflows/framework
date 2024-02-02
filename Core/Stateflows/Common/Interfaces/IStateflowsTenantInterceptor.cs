namespace Stateflows.Common
{
    public interface IStateflowsTenantInterceptor
    {
        bool BeforeExecute(string tenantId);

        void AfterExecute(string tenantId);
    }
}
