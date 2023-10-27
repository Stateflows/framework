using System.Collections.Generic;

namespace Stateflows.Activities
{
    public class ParametersToken : Token
    {
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
