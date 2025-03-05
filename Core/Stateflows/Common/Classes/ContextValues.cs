using System.Threading;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public static class ContextValues
    {
        public static void Clear()
        {
            GlobalValuesHolder.Value = null;
            StateValuesHolder.Value = null;
            ParentStateValuesHolder.Value = null;
            SourceStateValuesHolder.Value = null;
            TargetStateValuesHolder.Value = null;
        }
        
        internal static readonly AsyncLocal<IContextValues> GlobalValuesHolder = new AsyncLocal<IContextValues>();
        internal static bool AreGlobalValuesAvailable
            => GlobalValuesHolder.Value != null;
        public static void InitializeGlobalValues()
        {
            GlobalValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        public static IContextValues GlobalValues
            => GlobalValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> StateValuesHolder = new AsyncLocal<IContextValues>();
        internal static bool AreStateValuesAvailable
            => StateValuesHolder.Value != null;
        public static void InitializeStateValues()
        {
            StateValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        public static IContextValues StateValues
            => StateValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> ParentStateValuesHolder = new AsyncLocal<IContextValues>();
        internal static bool AreParentStateValuesAvailable
            => ParentStateValuesHolder.Value != null;
        public static void InitializeParentStateValues()
        {
            ParentStateValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        public static IContextValues ParentStateValues
            => ParentStateValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> SourceStateValuesHolder = new AsyncLocal<IContextValues>();
        internal static bool AreSourceStateValuesAvailable
            => SourceStateValuesHolder.Value != null;
        public static void InitializeSourceStateValues()
        {
            SourceStateValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        public static IContextValues SourceStateValues
            => SourceStateValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> TargetStateValuesHolder = new AsyncLocal<IContextValues>();
        internal static bool AreTargetStateValuesAvailable
            => TargetStateValuesHolder.Value != null;
        public static void InitializeTargetStateValues()
        {
            TargetStateValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        public static IContextValues TargetStateValues
            => TargetStateValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());
    }
}
