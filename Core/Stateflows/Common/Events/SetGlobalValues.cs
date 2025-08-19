using System.Collections.Generic;

namespace Stateflows.Common
{
    [NoImplicitInitialization, NoTracing]
    internal class SetGlobalValues : SystemEvent
    {
        public Dictionary<string, string> Values { get; set; }
    }
}
