using System;

namespace Stateflows
{
    [Obsolete]
    public static class StateflowsEnvironments
    {
        public static readonly string Production = nameof(Production);
        public static readonly string Development = nameof(Development);
        public static readonly string Staging = nameof(Staging);
    }
}
