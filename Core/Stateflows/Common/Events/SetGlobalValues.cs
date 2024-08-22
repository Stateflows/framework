using System.Collections.Generic;

namespace Stateflows.Common
{
    [NoImplicitInitialization]
    internal class SetGlobalValues : Event
    {
        public Dictionary<string, string> Values { get; set; }
    }
}
