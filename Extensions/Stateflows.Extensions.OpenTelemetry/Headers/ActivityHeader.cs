using System.Diagnostics;
using Stateflows.Common;

namespace Stateflows.Extensions.OpenTelemetry.Headers
{
    public class ActivityHeader : EventHeader
    {
        public ActivityContext ActivityContext { get; set; }
    }
}