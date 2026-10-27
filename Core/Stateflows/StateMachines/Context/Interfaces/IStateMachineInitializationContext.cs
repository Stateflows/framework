namespace Stateflows.StateMachines
{
    /// <summary>Provides machine context while initialization is attempted, before normal event processing.</summary>
    public interface IStateMachineInitializationContext : IStateMachineActionContext
    { }

    /// <summary>Provides the initialization payload accepted by a typed initializer.</summary>
    /// <typeparam name="TInitializationEvent">The initialization request type.</typeparam>
    public interface IStateMachineInitializationContext<out TInitializationEvent> : IStateMachineInitializationContext
    {
        /// <summary>The initialization event supplied to the initializer.</summary>
        TInitializationEvent InitializationEvent { get; }
    }
}
