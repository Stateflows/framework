using System.Threading;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public static class ContextValues
    {
        internal static readonly AsyncLocal<IContextValues> GlobalValuesHolder = new AsyncLocal<IContextValues>();
        public static IContextValues GlobalValues
            => GlobalValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> StateValuesHolder = new AsyncLocal<IContextValues>();
        public static IContextValues StateValues
            => StateValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> SourceStateValuesHolder = new AsyncLocal<IContextValues>();
        public static IContextValues SourceStateValues
            => SourceStateValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> TargetStateValuesHolder = new AsyncLocal<IContextValues>();
        public static IContextValues TargetStateValues
            => TargetStateValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());
    }
}
