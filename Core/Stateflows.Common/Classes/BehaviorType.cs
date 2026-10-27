namespace Stateflows
{
    /// <summary>Defines behavior kinds recognized by the Stateflows runtime.</summary>
    public static class BehaviorType
    {
        /// <summary>Behavior kind for an action.</summary>
        public const string Action = nameof(Action);
        /// <summary>Behavior kind for an activity.</summary>
        public const string Activity = nameof(Activity);
        /// <summary>Behavior kind for an entity.</summary>
        public const string Entity = nameof(Entity);
        /// <summary>Behavior kind for a state machine.</summary>
        public const string StateMachine = nameof(StateMachine);
    }
}
