namespace Stateflows.Entities.Context.Classes
{
    internal sealed class EntityInitializationExceptionContext(
        EntityActionContext actionContext,
        bool implicitInitialization) : DelegatingActionContext(actionContext), IEntityInitializationExceptionContext
    {
        public bool ImplicitInitialization => implicitInitialization;
    }
}
