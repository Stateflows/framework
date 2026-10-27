namespace Stateflows.Extensions.MinimalAPIs;

internal class EntityConfigurationVisitor(MinimalAPIsBuilder minimalApisBuilder)
    : Entities.EntityVisitor
{
    private BehaviorClass? OwnerClass = null;

    public override Task EntityAddingAsync<TTemplate>(string entityName, int entityVersion,
        BehaviorClass? ownerClass = null, BehaviorClass? parentClass = null, bool hasDefaultInstance = false)
    {
        OwnerClass = ownerClass;
        return Task.CompletedTask;
    }

    public override Task EntityAddedAsync<TTemplate>(string entityName, int entityVersion)
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
