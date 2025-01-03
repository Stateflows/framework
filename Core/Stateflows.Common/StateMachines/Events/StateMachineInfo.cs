﻿using Newtonsoft.Json;
using Stateflows.Common;
using System.Collections.Generic;

namespace Stateflows.StateMachines.Events
{
    public sealed class StateMachineInfo : BehaviorInfo
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<string> StatesStack { get; set; }
    }
}
