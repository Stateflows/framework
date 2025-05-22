using System;

namespace Stateflows
{
    [Obsolete]
    internal static class BehaviorClassDefaults
    {
        [Obsolete]
        public static string CurrentEnvironment { get; set; } = StateflowsEnvironments.Production;
    }
}
