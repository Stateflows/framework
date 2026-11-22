using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>Describes a direct field write requested by a <see cref="FieldState{T}"/> event.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TField">The declared field value type.</typeparam>
    public interface IFieldSetObservationContext<out TTemplate, out TField> : IBehaviorActionContext
    {
        /// <summary>Gets the live entity view. Observers must not change its state.</summary>
        TTemplate Entity { get; }

        /// <summary>Gets the registered name of the field being written.</summary>
        string FieldName { get; }

        /// <summary>Gets the requested value, which observers must not modify.</summary>
        TField Value { get; }
    }
}
