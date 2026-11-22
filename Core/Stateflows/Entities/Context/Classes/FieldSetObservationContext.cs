namespace Stateflows.Entities.Context.Classes
{
    internal sealed class FieldSetObservationContext<TTemplate, TField>(
        EntityActionContext actionContext,
        TTemplate entity,
        string fieldName,
        TField value)
        : DelegatingActionContext(actionContext), IFieldSetObservationContext<TTemplate, TField>
    {
        public TTemplate Entity { get; } = entity;

        public string FieldName { get; } = fieldName;

        public TField Value { get; } = value;
    }
}
