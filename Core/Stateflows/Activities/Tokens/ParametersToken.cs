using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public class ParametersToken : Token
    {
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
