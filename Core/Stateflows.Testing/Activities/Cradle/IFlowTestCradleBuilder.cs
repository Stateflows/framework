namespace Stateflows.Testing.Activities.Cradle
{
    public interface IFlowTestCradleBuilderWithInput
    {
        public IFlowTestCradleBuilderWithInput AddGlobalContextValue<T>(string name, T value);
        public ITestCradle Build();
    }

    public interface IFlowTestCradleBuilder<in TToken>
    {
        public IFlowTestCradleBuilderWithInput AddInputToken(TToken token);
    }
}
