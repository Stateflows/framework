namespace Stateflows.Entities
{
    public sealed class FieldValueResponse<TFieldValue> : FieldValueEvent
    {
        public TFieldValue FieldValue { get; set; }
    }
}
