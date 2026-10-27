namespace Stateflows.Entities
{
    /// <summary>Contains an entity projection result.</summary>
    /// <typeparam name="TProjection">The projection value type.</typeparam>
    public class ProjectionResponse<TProjection>
    {
        /// <summary>Gets or sets the projection value.</summary>
        public TProjection Projection { get; set; }
    }
}
