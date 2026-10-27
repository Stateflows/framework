using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>
    /// Provides the entity, computed field name, and executing behavior to an observer.
    /// </summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    public interface IComputedFieldObservationContext<out TTemplate> : IBehaviorActionContext
    {
        /// <summary>
        /// Gets the entity being evaluated. Observers must not mutate it.
        /// </summary>
        TTemplate Entity { get; }

        /// <summary>
        /// Gets the registered name of the computed field.
        /// </summary>
        string FieldName { get; }
    }
}
