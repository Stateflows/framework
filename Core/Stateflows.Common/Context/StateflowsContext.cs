using System.Collections.Generic;

namespace Stateflows.Common.Context
{
    public class StateflowsContext
    {
        public BehaviorId Id { get; set; }

        public int Version { get; set; } = 0;

        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        private Dictionary<string, string> globalValues = null;
        public Dictionary<string, string> GlobalValues
        {
            get
            {
                if (globalValues == null)
                {
                    if (!Values.TryGetValue(nameof(GlobalValues), out var globalValuesObj))
                    {
                        globalValues = new Dictionary<string, string>();
                        Values[nameof(GlobalValues)] = globalValues;
                    }
                    else
                    {
                        globalValues = globalValuesObj as Dictionary<string, string>;
                    }
                }

                return globalValues;
            }
        }
    }
}
