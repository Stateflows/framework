﻿using System.Threading;
using System.Collections.Generic;
using Stateflows.Common.Interfaces;

namespace Stateflows.Common.Classes
{
    public static class ContextValues
    {
        internal static AsyncLocal<IContextValues> GlobalValuesHolder { get; } = new AsyncLocal<IContextValues>();
        
        public static IContextValues GlobalValues
            => GlobalValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static AsyncLocal<IContextValues> StateValuesHolder = new AsyncLocal<IContextValues>();
        public static IContextValues StateValues
            => StateValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> ParentStateValuesHolder = new AsyncLocal<IContextValues>();
        public static IContextValues ParentStateValues
            => ParentStateValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> SourceStateValuesHolder = new AsyncLocal<IContextValues>();
        public static IContextValues SourceStateValues
            => SourceStateValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());

        internal static readonly AsyncLocal<IContextValues> TargetStateValuesHolder = new AsyncLocal<IContextValues>();
        public static IContextValues TargetStateValues
            => TargetStateValuesHolder.Value ??= new ContextValuesCollection(new Dictionary<string, string>());
    }
}
