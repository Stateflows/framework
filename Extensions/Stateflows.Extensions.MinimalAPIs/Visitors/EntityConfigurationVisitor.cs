namespace Stateflows.Extensions.MinimalAPIs;

internal class EntityConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder)
    : Entities.EntityVisitor
{
    private BehaviorClass? OwnerClass = null;

    public override Task EntityAddingAsync<TTemplate>(string id, string entityName, int entityVersion,
        BehaviorClass? ownerClass, BehaviorClass? parentClass, bool hasDefaultInstance)
    {
        OwnerClass = ownerClass;
        return Task.CompletedTask;
    }

    public override Task EntityAddedAsync<TTemplate>(string id, string entityName, int entityVersion)
    {
        if (OwnerClass != null)
        {
            minimalApisBuilder.ConfigureEntities(b =>
                b.ConfigureEntity(
                    entityName,
                    b => b.Disable()
                )
            );
        }

        return Task.CompletedTask;
    }
}
