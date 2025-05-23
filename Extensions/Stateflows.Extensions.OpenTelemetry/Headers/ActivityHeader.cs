using System.Diagnostics;
using Stateflows.Common;

namespace Stateflows.Extensions.OpenTelemetry.Headers
{
    public class ActivityHeader : EventHeader
    {
        public Activity Activity { get; set; } = null!;
    }
}