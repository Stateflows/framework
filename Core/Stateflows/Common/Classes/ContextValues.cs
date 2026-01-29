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
        
        internal static readonly AsyncLocal<IContextValues> GlobalValuesHolder = new();
        internal static bool AreGlobalValuesAvailable
            => GlobalValuesHolder.Value != null;
        public static void InitializeGlobalValues()
        {
            GlobalValuesHolder.Value = new StateflowsValuesCollection();
        }
        public static IContextValues GlobalValues
            => GlobalValuesHolder.Value;

        internal static readonly AsyncLocal<IContextValues> StateValuesHolder = new();
        internal static bool AreStateValuesAvailable
            => StateValuesHolder.Value != null;
        
        public static void InitializeStateValues()
        {
            StateValuesHolder.Value = new StateflowsValuesCollection();
        }
        
        public static IContextValues StateValues
            => StateValuesHolder.Value;

        internal static readonly AsyncLocal<IContextValues> ParentStateValuesHolder = new();
        internal static bool AreParentStateValuesAvailable
            => ParentStateValuesHolder.Value != null;
        public static void InitializeParentStateValues()
        {
            ParentStateValuesHolder.Value = new StateflowsValuesCollection();
        }
        public static IContextValues ParentStateValues
            => ParentStateValuesHolder.Value;

        internal static readonly AsyncLocal<IContextValues> SourceStateValuesHolder = new();
        internal static bool AreSourceStateValuesAvailable
            => SourceStateValuesHolder.Value != null;
        public static void InitializeSourceStateValues()
        {
            SourceStateValuesHolder.Value = new StateflowsValuesCollection();
        }
        public static IContextValues SourceStateValues
            => SourceStateValuesHolder.Value;

        internal static readonly AsyncLocal<IContextValues> TargetStateValuesHolder = new();
        internal static bool AreTargetStateValuesAvailable
            => TargetStateValuesHolder.Value != null;
        public static void InitializeTargetStateValues()
        {
            TargetStateValuesHolder.Value = new StateflowsValuesCollection();
        }
        public static IContextValues TargetStateValues
            => TargetStateValuesHolder.Value;
    }
}
