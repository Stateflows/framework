using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Entities.Attributes;

namespace Entity.IntegrationTests.Tests;

public enum SerializationStatus
{
    Pending,
    Complete
}

public record SetSerializedFieldValues(SerializationStatus Status, int Number);

public interface IFieldSerializationEntityTemplate
{
    [Field(FieldAccess.Get | FieldAccess.Set)]
    SerializationStatus Status { get; set; }

    [Field(FieldAccess.Get | FieldAccess.Set)]
    int? Number { get; set; }
}

[TestClass]
public class FieldSerialization : StateflowsTestClass
{
    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddEntities(entities => entities
            .AddEntity<IFieldSerializationEntityTemplate>("fieldSerialization", entity => entity
                .AddMutation<SetSerializedFieldValues>(context =>
                {
                    context.Entity.Status = context.MutationEvent.Status;
                    context.Entity.Number = context.MutationEvent.Number;
                })
            )
        );
    }

    [TestMethod]
    public async Task EntityFieldsPreserveEnumAndNullableIntegerValuesAcrossSerialization()
    {
        Assert.IsTrue(ServiceProvider.GetRequiredService<IEntityLocator>()
            .TryLocateEntity(new EntityClass("fieldSerialization").ToId("x"), out var entity));

        var mutation = await entity.SendAsync(new SetSerializedFieldValues(SerializationStatus.Complete, 42));
        Assert.AreEqual(EventStatus.Consumed, mutation.Status);

        var (statusRead, status) = await entity.TryGetAsync<SerializationStatus>(nameof(IFieldSerializationEntityTemplate.Status));
        var (numberRead, number) = await entity.TryGetAsync<int?>(nameof(IFieldSerializationEntityTemplate.Number));

        Assert.IsTrue(statusRead);
        Assert.AreEqual(SerializationStatus.Complete, status);
        Assert.IsTrue(numberRead);
        Assert.AreEqual(42, number);
    }
}
