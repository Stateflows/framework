using Stateflows;
using Stateflows.Common;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class StateflowsGrainKeysTests
{
    [TestMethod]
    [DataRow("host", "orders", "42")]
    [DataRow("{\"tenant\":1}", "na/me|x", "in%st{an}ce/#")]
    [DataRow("", "name", "")]
    [DataRow(null, "name", "instance")]
    [DataRow("%", "%", "%")]
    public void BehaviorKeysRoundTripAndContainNoFormatOrJsonCharacters(string? tenant, string name, string instance)
    {
        var id = new BehaviorId(BehaviorType.StateMachine, name, instance);

        var key = StateflowsGrainKeys.ForBehavior(tenant, id);
        var parsed = StateflowsGrainKeys.ParseBehavior(key);

        Assert.AreEqual(tenant, parsed.TenantId);
        Assert.AreEqual(id, parsed.BehaviorId);
        Assert.AreEqual(id.Instance, parsed.BehaviorId.Instance);
        Assert.IsFalse(key.IndexOfAny(['{', '}', '"', '|']) >= 0, key);
        Assert.AreEqual(key, string.Format(key), "Orleans ADO.NET storage formats keys with string.Format.");

        var classKey = StateflowsGrainKeys.ForBehaviorClass(tenant, id.BehaviorClass);
        var parsedClass = StateflowsGrainKeys.ParseBehaviorClass(classKey);
        Assert.AreEqual(tenant, parsedClass.TenantId);
        Assert.AreEqual(id.BehaviorClass, parsedClass.BehaviorClass);
        Assert.AreEqual(classKey, string.Format(classKey));
    }

    [TestMethod]
    public void KeysAreReadableAndDistinguishNullEmptyAndPercentTenants()
    {
        var id = new BehaviorId(BehaviorType.Activity, "orders", "42");
        Assert.AreEqual("host/Activity/orders/42", StateflowsGrainKeys.ForBehavior("host", id));
        CollectionAssert.AllItemsAreUnique(new[]
        {
            StateflowsGrainKeys.ForBehavior(null, id),
            StateflowsGrainKeys.ForBehavior(string.Empty, id),
            StateflowsGrainKeys.ForBehavior("%", id),
        });
    }

    [TestMethod]
    public void DeliveryKeysRoundTrip()
    {
        var publisher = StateflowsGrainKeys.ForBehavior("t|1", new BehaviorId(BehaviorType.StateMachine, "p|x", "1"));
        var subscriber = StateflowsGrainKeys.ForBehavior("t|1", new BehaviorId(BehaviorType.Entity, "s", "2|3"));

        var key = StateflowsGrainKeys.ForDelivery(publisher, subscriber);

        Assert.AreEqual((publisher, subscriber), StateflowsGrainKeys.ParseDelivery(key));
        Assert.AreEqual(key, string.Format(key));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("host/StateMachine/name")]
    [DataRow("host/StateMachine/name/1/extra")]
    [DataRow("host//name/1")]
    [DataRow("{\"TenantId\":\"host\"}")]
    public void MalformedBehaviorKeysAreRejected(string key)
        => Assert.ThrowsExactly<ArgumentException>(() => StateflowsGrainKeys.ParseBehavior(key));

    [TestMethod]
    [DataRow("")]
    [DataRow("a")]
    [DataRow("a|")]
    [DataRow("a|b|c")]
    public void MalformedDeliveryKeysAreRejected(string key)
        => Assert.ThrowsExactly<ArgumentException>(() => StateflowsGrainKeys.ParseDelivery(key));
}
