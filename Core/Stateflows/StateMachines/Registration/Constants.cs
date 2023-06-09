﻿namespace Stateflows.StateMachines.Registration
{
    internal class Constants
    {
        public static readonly string StatesStack = nameof(StatesStack);
        public static readonly string ForceConsumed = nameof(ForceConsumed);
        public static readonly string DefaultTransitionTarget = string.Empty;
        public static readonly string CompletionEvent = string.Empty;
        public static readonly string SourceState = nameof(SourceState);
        public static readonly string TargetState = nameof(TargetState);
        public static readonly string Event = nameof(Event);
        public static readonly string StateMachine = nameof(StateMachine);
        public static readonly string State = nameof(State);
        public static readonly string StateValues = nameof(StateValues);
        public static readonly string GlobalValues = nameof(GlobalValues);
        public static readonly string DeferredEvents = nameof(DeferredEvents);
        public static readonly string TimeEventIds = nameof(TimeEventIds);
        public static readonly string SubmachineId = nameof(SubmachineId);
        public static readonly string Entry = nameof(Entry);
        public static readonly string OnEntry = nameof(OnEntry);
        public static readonly string OnEntryAsync = nameof(OnEntryAsync);
        public static readonly string Initialize = nameof(Initialize);
        public static readonly string OnInitialize = nameof(OnInitialize);
        public static readonly string OnInitializeAsync = nameof(OnInitializeAsync);
        public static readonly string Exit = nameof(Exit);
        public static readonly string OnExit = nameof(OnExit);
        public static readonly string OnExitAsync = nameof(OnExitAsync);
        public static readonly string Guard = nameof(Guard);
        public static readonly string GuardAsync = nameof(GuardAsync);
        public static readonly string Effect = nameof(Effect);
        public static readonly string EffectAsync = nameof(EffectAsync);
    }
}