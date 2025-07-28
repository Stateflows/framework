using System;

namespace Stateflows
{
    [Obsolete("This class will be removed in a future release.")]
    internal static class BehaviorClassDefaults
    {
        [Obsolete("This property will be removed in a future release.")]
        public static string CurrentEnvironment { get; set; } = StateflowsEnvironments.Production;
    }
}
