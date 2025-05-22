using System.Collections.Generic;

namespace Stateflows.Common
{
    [NoImplicitInitialization, NoTracing]
    public sealed class ContextValuesResponse
    {
        public Dictionary<string, string> GlobalValues { get; set; }
        public Dictionary<string, Dictionary<string, string>> StateValues { get; set; }
    }
}
