using System.Threading;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    /// <summary>Exposes value collections associated with the current asynchronous execution context.</summary>
    /// <remarks>Each collection must be initialized for its matching scope before it can be used.</remarks>
    public static class ContextValues
    {
        /// <summary>Clears the current global and state-related value collections.</summary>
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
        /// <summary>Creates an empty global value collection for the current asynchronous execution context.</summary>
        public static void InitializeGlobalValues()
        {
            GlobalValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        /// <summary>Gets the current global value collection, when initialized.</summary>
        public static IContextValues GlobalValues
            => GlobalValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> StateValuesHolder = new();
        internal static bool AreStateValuesAvailable
            => StateValuesHolder.Value != null;
        /// <summary>Creates an empty state value collection for the current asynchronous execution context.</summary>
        public static void InitializeStateValues()
        {
            StateValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        /// <summary>Gets the current state value collection, when initialized.</summary>
        public static IContextValues StateValues
            => StateValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> ParentStateValuesHolder = new();
        internal static bool AreParentStateValuesAvailable
            => ParentStateValuesHolder.Value != null;
        /// <summary>Creates an empty parent-state value collection for the current asynchronous execution context.</summary>
        public static void InitializeParentStateValues()
        {
            ParentStateValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        /// <summary>Gets the current parent-state value collection, when initialized.</summary>
        public static IContextValues ParentStateValues
            => ParentStateValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> SourceStateValuesHolder = new();
        internal static bool AreSourceStateValuesAvailable
            => SourceStateValuesHolder.Value != null;
        /// <summary>Creates an empty source-state value collection for the current asynchronous execution context.</summary>
        public static void InitializeSourceStateValues()
        {
            SourceStateValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        /// <summary>Gets the current source-state value collection, when initialized.</summary>
        public static IContextValues SourceStateValues
            => SourceStateValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> TargetStateValuesHolder = new();
        internal static bool AreTargetStateValuesAvailable
            => TargetStateValuesHolder.Value != null;
        /// <summary>Creates an empty target-state value collection for the current asynchronous execution context.</summary>
        public static void InitializeTargetStateValues()
        {
            TargetStateValuesHolder.Value = new ContextValuesCollection(new Dictionary<string, string>());
        }
        /// <summary>Gets the current target-state value collection, when initialized.</summary>
        public static IContextValues TargetStateValues
            => TargetStateValuesHolder.Value;// ??= new ContextValuesCollection(new Dictionary<string, string>());
    }
}
