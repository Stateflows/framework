using System.Collections.Generic;

namespace Stateflows.Testing.Activities.Cradle
{
    public interface ITestResults
    {
        bool IsGlobalContextValueSet(string key);
        public bool TryGetGlobalContextValue<T>(string key, out T value);
        public IEnumerable<T> GetOutputTokens<T>();
    }
}
