using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class OutputNotificationSerializationTests
{
    [TestMethod]
    public void RawOutputPayloadTypesAndMetadataSurviveTransport()
    {
        AssertRoundTrip(42);
        AssertRoundTrip(42L);
        AssertRoundTrip(1.25);
        AssertRoundTrip(1.25m);
        AssertRoundTrip("text");
        AssertRoundTrip<string?>(null);
        AssertRoundTrip<int?>(null);
        AssertRoundTrip<IOutput>(new Output { Value = 7 });
    }

    private static void AssertRoundTrip<T>(T payload)
    {
        var source = payload.ToEventHolder(new ActivityId("output", "a"));
        source.Retained = true;
        source.TimeToLive = 37;
        source.SentAt = DateTime.Now;
        source.Headers[nameof(ImmediatePublish)] = new ImmediatePublish();
        OrleansEventHolder transport = source;
        EventHolder restored = transport;
        Assert.IsInstanceOfType<EventHolder<T>>(restored);
        Assert.AreEqual(source.Name, restored.Name);
        Assert.AreEqual(source.Id, restored.Id);
        Assert.AreEqual(source.SenderId, restored.SenderId);
        Assert.AreEqual(source.SentAt, restored.SentAt);
        Assert.IsTrue(restored.Retained);
        Assert.AreEqual(37, restored.TimeToLive);
        Assert.IsInstanceOfType<ImmediatePublish>(restored.Headers[nameof(ImmediatePublish)]);
        if (payload is IOutput output)
            Assert.AreEqual(output.Value, ((EventHolder<T>)restored).Payload is IOutput result ? result.Value : -1);
        else
            Assert.AreEqual(payload, ((EventHolder<T>)restored).Payload);
    }

    [TestMethod]
    public void GenericTransportDeserializesRawPrimitiveUsingItsDeclaredType()
    {
        OrleansEventHolder<int> transport = 42.ToEventHolder();
        EventHolder<int> restored = transport;
        Assert.AreEqual(42, restored.Payload);
    }

    [TestMethod]
    public void ExistingTransportEnvelopesRemainReadable()
    {
        OrleansEventHolder transport = new Output { Value = 7 }.ToEventHolder();
        transport.PayloadType = null;
        EventHolder restored = transport;
        Assert.AreEqual(7, ((EventHolder<Output>)restored).Payload.Value);
    }

    public interface IOutput
    {
        int Value { get; }
    }

    public sealed class Output : IOutput
    {
        public int Value { get; set; }
    }
}
