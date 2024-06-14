using System.Collections.Generic;

namespace Stateflows.Testing.Activities.Cradle
{
    public interface INodeTestCradleBuilder
    {
        public INodeTestCradleBuilder AddGlobalContextValue<T>(string name, T value);
        public INodeTestCradleBuilder AddInputToken<T>(T token);
        public INodeTestCradleBuilder AddInputTokens<T>(IEnumerable<T> tokens);
        public ITestCradle Build();
    }
}
