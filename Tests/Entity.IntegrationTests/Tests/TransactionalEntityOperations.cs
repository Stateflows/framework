namespace Entity.IntegrationTests.Tests;

[TestClass]
public class TransactionalEntityOperations
{
    [TestMethod]
    [DataRow(true, false)]
    [DataRow(true, true)]
    [DataRow(false, false)]
    [DataRow(false, true)]
    public async Task TransactionalOperationsForwardArgumentsAndEnsureJoinHeader(bool mutate, bool useTemplate)
    {
        foreach (var result in new[] { true, false })
        {
            var unrelatedHeader = new TestHeader();
            var existingJoinHeader = new JoinTransaction();
            IDictionary<string, EventHeader>?[] headerCases =
            [
                null,
                new Dictionary<string, EventHeader>(),
                new Dictionary<string, EventHeader> { ["unrelated"] = unrelatedHeader },
                new Dictionary<string, EventHeader> { [nameof(JoinTransaction)] = existingJoinHeader },
                new Dictionary<string, EventHeader> { ["customJoinKey"] = existingJoinHeader }
            ];

            foreach (var headers in headerCases)
            {
                var originalHeaders = headers?.ToArray();
                var originalJoinHeader = headers?.Values.OfType<JoinTransaction>().SingleOrDefault();
                var recorder = new RecordingEntityOperations(result);
                IEntityOperations<object> operations = recorder;
                var task = mutate
                    ? useTemplate
                        ? operations.TryMutateTransactionallyAsync<ITestEntity, int>(42, headers)
                        : operations.TryMutateTransactionallyAsync("entity", 42, headers)
                    : useTemplate
                        ? operations.TrySetFieldTransactionallyAsync<ITestEntity, int>("field", 42, headers)
                        : operations.TrySetFieldTransactionallyAsync("entity", "field", 42, headers);

                Assert.AreSame(recorder.Result, task);
                Assert.AreEqual(result, await task);
                Assert.AreEqual(1, recorder.CallCount);
                Assert.AreEqual(mutate, recorder.Mutated);
                Assert.AreEqual(useTemplate ? Entity<ITestEntity>.Name : "entity", recorder.EntityName);
                Assert.AreEqual(mutate ? null : "field", recorder.FieldName);
                Assert.AreEqual(42, recorder.Value);
                Assert.AreEqual(typeof(int), recorder.ValueType);
                Assert.IsNotNull(recorder.Headers);
                Assert.AreEqual(1, recorder.Headers.Values.Count(header => header is JoinTransaction));

                if (headers is not null)
                {
                    Assert.AreSame(headers, recorder.Headers);
                    foreach (var entry in originalHeaders!)
                    {
                        Assert.AreSame(entry.Value, recorder.Headers[entry.Key]);
                    }
                }

                if (originalJoinHeader is not null)
                {
                    Assert.AreSame(originalJoinHeader, recorder.Headers.Values.OfType<JoinTransaction>().Single());
                    Assert.AreEqual(originalHeaders!.Length, recorder.Headers.Count);
                }
                else
                {
                    Assert.IsInstanceOfType<JoinTransaction>(recorder.Headers[nameof(JoinTransaction)]);
                }
            }
        }
    }

    public interface ITestEntity
    {
    }

    private sealed class TestHeader : EventHeader
    {
    }

    private sealed class RecordingEntityOperations(bool result) : IEntityOperations<object>
    {
        public Task<bool> Result { get; } = Task.FromResult(result);
        public int CallCount { get; private set; }
        public bool Mutated { get; private set; }
        public string? EntityName { get; private set; }
        public string? FieldName { get; private set; }
        public object? Value { get; private set; }
        public Type? ValueType { get; private set; }
        public IDictionary<string, EventHeader>? Headers { get; private set; }

        public Task<bool> TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers = null)
        {
            CallCount++;
            Mutated = true;
            EntityName = entityName;
            Value = mutationEvent;
            ValueType = typeof(TMutationEvent);
            Headers = headers;
            return Result;
        }

        public Task<bool> TrySetFieldAsync<TFieldType>(string entityName, string fieldName, TFieldType fieldValue, IDictionary<string, EventHeader>? headers = null)
        {
            CallCount++;
            EntityName = entityName;
            FieldName = fieldName;
            Value = fieldValue;
            ValueType = typeof(TFieldType);
            Headers = headers;
            return Result;
        }

        public Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers = null)
            => throw new NotSupportedException();

        public Task<(bool Success, TFieldType Field)> TryGetFieldAsync<TFieldType>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers = null)
            => throw new NotSupportedException();
    }
}
