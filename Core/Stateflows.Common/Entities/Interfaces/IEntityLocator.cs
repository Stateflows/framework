namespace Stateflows.Entities
{
    public interface IEntityLocator
    {
        public bool TryLocateEntity(EntityId id, out IEntityBehavior stateMachine);
    }
}
