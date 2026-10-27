using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Stateflows.Common
{
    /// <summary>Describes a behavior's lifecycle state and published metadata.</summary>
    [Retain, StrictOwnership]
    public class BehaviorInfo
    {
        /// <summary>Gets or sets the identifier of the described behavior instance.</summary>
        public BehaviorId Id { get; set; }
        
        /// <summary>Gets or sets the behavior's lifecycle status.</summary>
        public BehaviorStatus BehaviorStatus { get; set; }
        
        /// <summary>Gets the enum name corresponding to <see cref="BehaviorStatus"/>.</summary>
        public string BehaviorStatusText => Enum.GetName(typeof(BehaviorStatus), BehaviorStatus);

        /// <summary>Gets or sets names of events currently expected by the behavior.</summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<string> ExpectedEvents { get; set; }

        /// <summary>Gets or sets user-defined behavior metadata.</summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        /// <summary>Gets or sets behavior properties exposed for inspection.</summary>
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}
