namespace Stateflows.Entities
{
    /// <summary>Locates behavior handles for entity instances.</summary>
    public interface IEntityLocator
    {
        /// <summary>Attempts to locate the entity with the specified identifier.</summary>
        /// <param name="id">The identifier of the entity instance.</param>
        /// <param name="stateMachine">Receives the entity behavior handle when found.</param>
        /// <returns><see langword="true"/> when a provider can supply the entity; otherwise, <see langword="false"/>.</returns>
        public bool TryLocateEntity(EntityId id, out IEntityBehavior stateMachine);
    }
}
