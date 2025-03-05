using System.Collections.Generic;

namespace Stateflows.Common
{
    [NoImplicitInitialization]
    internal class SetGlobalValues : SystemEvent
    {
        public Dictionary<string, string> Values { get; set; }
    }
}
