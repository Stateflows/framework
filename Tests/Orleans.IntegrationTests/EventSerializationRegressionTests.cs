using Newtonsoft.Json;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class EventSerializationRegressionTests
{
    [TestMethod]
    public void AmbiguousConstructorsFailDuringTransportReconstruction()
    {
        OrleansEventHolder wire = new AmbiguousFailure(new List<string> { "failure" }).ToEventHolder();

        var exception = Assert.ThrowsExactly<JsonSerializationException>(() =>
        {
            EventHolder restored = wire;
        });

        StringAssert.Contains(exception.Message, "Unable to find a constructor");
        StringAssert.Contains(exception.Message, typeof(AmbiguousFailure).FullName!);
    }

    [TestMethod]
    public void ExistingEnvelopeWithHeadersFailsDuringCoreCloneBeforeTransport()
    {
        EventHolder source = new AmbiguousFailure(new List<string> { "failure" }).ToEventHolder();

        var exception = Assert.ThrowsExactly<JsonSerializationException>(() =>
            source.ToTypedEventHolder(new Dictionary<string, EventHeader>()));

        StringAssert.Contains(exception.Message, "Unable to find a constructor");
        StringAssert.Contains(exception.Path, "Payload");
    }

    [TestMethod]
    public void SingleMatchingConstructorPreservesReadOnlyCollection()
    {
        var restored = RoundTrip(new MatchingFailure(["failure"], "effect"));

        CollectionAssert.AreEqual(new[] { "failure" }, restored.Messages);
        Assert.AreEqual("effect", restored.MethodContext);
    }

    [TestMethod]
    public void SelectedMatchingConstructorPreservesBothCollections()
    {
        var restored = RoundTrip(new SelectedFailure(
            [new InvalidOperationException("exception", new ArgumentException("inner"))],
            ["failure"], "effect"));

        CollectionAssert.AreEqual(new[] { "failure" }, restored.Messages);
        Assert.AreEqual("effect", restored.MethodContext);
        var exception = restored.Exceptions.Single();
        Assert.IsInstanceOfType<InvalidOperationException>(exception);
        Assert.AreEqual("exception", exception.Message);
        Assert.IsInstanceOfType<ArgumentException>(exception.InnerException);
        Assert.AreEqual("inner", exception.InnerException!.Message);
    }

    private static T RoundTrip<T>(T payload)
    {
        OrleansEventHolder wire = payload.ToEventHolder();
        EventHolder restored = wire;
        return ((EventHolder<T>)restored).Payload;
    }

    public sealed class AmbiguousFailure
    {
        public AmbiguousFailure(List<string> errorMessages)
        {
            Messages = errorMessages;
            Exceptions = [];
        }

        public AmbiguousFailure(List<Exception> errorList)
        {
            Exceptions = errorList;
            Messages = [];
        }

        public List<string> Messages { get; }
        public List<Exception> Exceptions { get; }
    }

    public sealed class UnmatchedFailure(List<string> errorMessages, string? methodContext = null)
    {
        public List<string> Messages { get; } = errorMessages;
        public string? MethodContext { get; } = methodContext;
    }

    public sealed class AnnotatedUnmatchedFailure
    {
        [JsonConstructor]
        public AnnotatedUnmatchedFailure(List<string> errorMessages, string? methodContext = null)
        {
            Messages = errorMessages;
            MethodContext = methodContext;
        }

        public AnnotatedUnmatchedFailure(List<Exception> errorList)
        {
            Messages = errorList.Select(exception => exception.Message).ToList();
        }

        public List<string> Messages { get; }
        public string? MethodContext { get; }
    }

    public sealed class NestedFailure
    {
        public UnmatchedFailure? Failure { get; set; }
    }

    public sealed class MatchingFailure(List<string> messages, string? methodContext = null)
    {
        public List<string> Messages { get; } = messages;
        public string? MethodContext { get; } = methodContext;
    }

    public sealed class SelectedFailure
    {
        [JsonConstructor]
        public SelectedFailure(List<Exception> exceptions, List<string> messages, string? methodContext)
        {
            Exceptions = exceptions;
            Messages = messages;
            MethodContext = methodContext;
        }

        public SelectedFailure(List<string> errorMessages)
            : this([], errorMessages, null)
        { }

        public List<Exception> Exceptions { get; }
        public List<string> Messages { get; }
        public string? MethodContext { get; }
    }
}
