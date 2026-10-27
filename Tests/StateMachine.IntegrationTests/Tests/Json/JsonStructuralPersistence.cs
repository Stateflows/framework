using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Utilities;
using Stateflows.Events;
using Stateflows.Events.Json;

namespace StateMachine.IntegrationTests.Tests.Json;

[TestClass]
[TestCategory("JsonStructuralEvents")]
public class JsonStructuralPersistence
{
    [TestMethod]
    [DataRow(FieldType.String)]
    [DataRow(FieldType.Boolean)]
    [DataRow(FieldType.Int32)]
    [DataRow(FieldType.Int64)]
    [DataRow(FieldType.Decimal)]
    [DataRow(FieldType.Double)]
    [DataRow(FieldType.Guid)]
    [DataRow(FieldType.DateTime)]
    [DataRow(FieldType.DateTimeOffset)]
    [DataRow(FieldType.TimeSpan)]
    [DataRow(FieldType.DateOnly)]
    [DataRow(FieldType.TimeOnly)]
    [DataRow(FieldType.Binary)]
    [DataRow(FieldType.Enum)]
    public void AllScalarTypesSurvivePolymorphicPersistenceAtEveryDepth(FieldType type)
    {
        var (payload, expected) = Scalar(type);
        var document = Definition(type);
        using var host = Host(document);
        var factory = host.GetRequiredService<IEventFactory>();
        var original = factory.CreateFromJson("persisted", $$"""
            {
              "value":{{payload}},
              "values":[{{payload}},null],
              "child":{"value":{{payload}},"values":[{{payload}},null]},
              "children":[{"value":{{payload}},"values":[{{payload}},null]},null],
              "nullable":null
            }
            """);

        foreach (var camelCase in new[] { false, true })
        {
            var serialized = StateflowsJsonConverter.SerializePolymorphicObject(original, camelCase);
            var restored = (StructuralEvent)StateflowsJsonConverter.DeserializeObject(serialized);
            Assert.AreEqual(original.GetType(), restored.GetType());
            Assert.AreEqual("persisted", restored.Name);
            AssertValues(restored, expected);
            AssertValues(restored.Get<StructuralValue>("child"), expected);
            var children = restored.GetCollection<StructuralValue>("children");
            Assert.AreEqual(2, children.Count);
            AssertValues(children[0], expected);
            Assert.IsNull(children[1]);
            Assert.IsTrue(restored.ContainsField("nullable"));
            Assert.IsNull(restored.Get<object>("nullable"));
            Assert.IsFalse(restored.ContainsField("missing"));

            // Reconstructing checks that persisted CLR types still satisfy the strict factory.
            var recreated = factory.Create(restored.Name, restored.Fields);
            AssertValues(recreated, expected);
            AssertValues(recreated.Get<StructuralValue>("child"), expected);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void RequestResponseIdentityAndPayloadSurvivePersistence(bool clrResponse)
    {
        var document = Definition(FieldType.Decimal);
        document["response"] = clrResponse
            ? new JsonObject { ["clrType"] = "host.response" }
            : new JsonObject { ["eventName"] = "reply" };
        using var host = Host(document, new JsonOptions().AddType<string>("host.response"));
        var request = host.GetRequiredService<IEventFactory>().CreateFromJson("persisted",
            """{"value":1234567890.1234567890123456789,"values":[null,1234567890.1234567890123456789]}""");
        var serialized = StateflowsJsonConverter.SerializePolymorphicObject(request);
        var restored = (StructuralRequest)StateflowsJsonConverter.DeserializeObject(serialized);
        Assert.AreEqual("persisted", restored.Name);
        Assert.AreEqual(clrResponse ? typeof(string) : null, restored.ResponseType);
        Assert.AreEqual(clrResponse ? null : "reply", restored.ResponseEventName);
        AssertScalar(1234567890.1234567890123456789m, restored.Get<decimal>("value"));
        var values = restored.GetCollection<decimal?>("values");
        Assert.IsNull(values[0]);
        Assert.AreEqual(1234567890.1234567890123456789m, values[1]);
    }

    [TestMethod]
    [DataRow("2026-10-23T01:02:03.1234567Z", DateTimeKind.Utc)]
    [DataRow("2026-10-23T01:02:03.1234567", DateTimeKind.Unspecified)]
    public void DateTimePersistencePreservesKindAndTicks(string text, DateTimeKind kind)
    {
        using var host = Host(Definition(FieldType.DateTime));
        var original = host.GetRequiredService<IEventFactory>().CreateFromJson("persisted",
            JsonSerializer.Serialize(new { value = text }));
        var restored = (StructuralEvent)StateflowsJsonConverter.DeserializeObject(
            StateflowsJsonConverter.SerializePolymorphicObject(original));
        var actual = restored.Get<DateTime>("value");
        Assert.AreEqual(kind, actual.Kind);
        Assert.AreEqual(1234567L, actual.Ticks % TimeSpan.TicksPerSecond);
        Assert.AreEqual(original.Get<DateTime>("value").Ticks, actual.Ticks);
    }

    [TestMethod]
    public void AliasFactoriesAreExplicitCaseSensitiveAndCannotLoadTypes()
    {
        var document = Definition(FieldType.String);
        document["fields"]![0]!["validation"] = JsonNode.Parse("""
            [{"type":"Custom","alias":"host.rule","options":{"minimum":3}}]
            """);
        var calls = 0;
        var options = new JsonOptions().AddValidator("host.rule", configuration =>
        {
            calls++;
            Assert.AreEqual(JsonValueKind.Object, configuration.ValueKind);
            return new MinLengthAttribute(configuration.GetProperty("minimum").GetInt32());
        });
        var parsed = JsonEventDefinition.Parse(document.ToJsonString(), options);
        Assert.AreEqual(1, calls);
        var services = new ServiceCollection();
        new StateflowsBuilder(services).AddEvents(events => parsed.Register(events));
        using var host = services.BuildServiceProvider();
        var attribute = host.GetRequiredService<IEventsRegister>().GetEvent("persisted").Fields["value"].Validation.Single();
        Assert.IsFalse(attribute.IsValid("ab"));
        Assert.IsTrue(attribute.IsValid("abc"));
        Assert.AreEqual(1, calls);

        foreach (var alias in new[] { "HOST.RULE", "System.ComponentModel.DataAnnotations.RequiredAttribute",
            "System.ComponentModel.DataAnnotations.RequiredAttribute, System.ComponentModel.Annotations" })
        {
            document["fields"]![0]!["validation"]![0]!["alias"] = alias;
            Assert.Throws<JsonException>(() => JsonEventDefinition.Parse(document.ToJsonString(), options));
        }
        Assert.AreEqual(1, calls);
        Assert.Throws<ArgumentException>(() => options.AddValidator("host.rule", _ => new RequiredAttribute()));
        Assert.Throws<ArgumentException>(() => options.AddValidator(" ", _ => new RequiredAttribute()));
        Assert.Throws<ArgumentNullException>(() => options.AddValidator("null", null!));
    }

    [TestMethod]
    [DataRow("""{"type":"StringLength","options":{}}""")]
    [DataRow("""{"type":"StringLength","options":{"maximumLength":1,"minimumLength":2}}""")]
    [DataRow("""{"type":"MinLength","options":{}}""")]
    [DataRow("""{"type":"MaxLength","options":{"length":-1}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"System.Int32","minimum":0,"maximum":1}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Int32","minimum":0,"maximum":2147483648}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Decimal","minimum":"0","maximum":1}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Double","minimum":0,"maximum":1e400}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Int64","minimum":2,"maximum":1}}""")]
    [DataRow("""{"type":"RegularExpression","options":{"pattern":"["}}""")]
    [DataRow("""{"type":"RegularExpression","options":{"pattern":""}}""")]
    [DataRow("""{"type":"RegularExpression","options":{"pattern":"x","matchTimeoutInMilliseconds":0}}""")]
    [DataRow("""{"type":"Required","options":{"allowEmptyStrings":"true"}}""")]
    [DataRow("""{"type":"EmailAddress","options":{"pattern":"x"}}""")]
    [DataRow("""{"type":"Custom","alias":"host.rule","clrType":"System.String"}""")]
    [DataRow("""{"type":"Required","alias":"host.rule"}""")]
    [DataRow("""{"type":"Custom","alias":"host.rule","options":null}""")]
    public void InvalidValidationDefinitionsAreJsonErrors(string validation)
    {
        var document = Definition(FieldType.String);
        document["fields"]![0]!["validation"] = new JsonArray(JsonNode.Parse(validation));
        Assert.Throws<JsonException>(() => JsonEventDefinition.Parse(document.ToJsonString(),
            new JsonOptions().AddValidator("host.rule", _ => new RequiredAttribute())));
    }

    [TestMethod]
    public void PayloadMetadataIsNotInterpretedAsClrTypes()
    {
        using var host = Host(Definition(FieldType.String));
        var result = host.GetRequiredService<IEventFactory>().CreateFromJson("persisted", """
            {
              "$type":"System.String, System.Private.CoreLib",
              "value":"2026-10-23T01:02:03Z",
              "child":{"value":"nested","$type":"System.IO.FileInfo, System.Private.CoreLib"}
            }
            """);
        Assert.AreEqual("2026-10-23T01:02:03Z", result.Get<string>("value"));
        Assert.AreEqual("nested", result.Get<StructuralValue>("child").Get<string>("value"));
        Assert.AreEqual(2, result.Fields.Count);
        Assert.AreEqual(1, result.Get<StructuralValue>("child").Fields.Count);
    }

    [TestMethod]
    public void InvalidBinaryPayloadReportsTheNestedFieldPath()
    {
        using var host = Host(Definition(FieldType.Binary));
        var exception = Assert.Throws<JsonException>(() => host.GetRequiredService<IEventFactory>()
            .CreateFromJson("persisted", """{"children":[{"value":"AQ ID"}]}"""));
        StringAssert.Contains(exception.Message, "$.children[0].value");
    }

    private static void AssertValues(StructuralValue value, object expected)
    {
        AssertScalar(expected, value.Fields["value"]);
        var values = value.Get<IList>("values");
        Assert.AreEqual(2, values.Count);
        AssertScalar(expected, values[0]!);
        Assert.IsNull(values[1]);
    }

    private static void AssertScalar(object expected, object actual)
    {
        Assert.AreEqual(expected.GetType(), actual.GetType(), "The declared CLR scalar type must be preserved.");
        switch (expected)
        {
            case byte[] bytes:
                CollectionAssert.AreEqual(bytes, (byte[])actual);
                break;
            case DateTimeOffset date:
                Assert.IsTrue(date.EqualsExact((DateTimeOffset)actual), "Offset and local ticks must be preserved.");
                break;
            case DateTime date:
                Assert.AreEqual(date.Kind, ((DateTime)actual).Kind);
                Assert.AreEqual(date.Ticks, ((DateTime)actual).Ticks);
                break;
            case decimal number:
                CollectionAssert.AreEqual(decimal.GetBits(number), decimal.GetBits((decimal)actual),
                    "Decimal precision and scale must be preserved.");
                break;
            case double number:
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(number), BitConverter.DoubleToInt64Bits((double)actual));
                break;
            default:
                Assert.AreEqual(expected, actual);
                break;
        }
    }

    private static (string Json, object Value) Scalar(FieldType type) => type switch
    {
        FieldType.String => ("\"2026-10-23T01:02:03.1234567+05:45\"", "2026-10-23T01:02:03.1234567+05:45"),
        FieldType.Boolean => ("true", true),
        FieldType.Int32 => ("-2147483648", int.MinValue),
        FieldType.Int64 => ("9223372036854775807", long.MaxValue),
        FieldType.Decimal => ("1234567890.1234567890123456789", 1234567890.1234567890123456789m),
        FieldType.Double => ("1.2345678901234567", 1.2345678901234567d),
        FieldType.Guid => ("\"d4619b50-34f7-410a-a06a-d3cd991bb746\"", Guid.Parse("d4619b50-34f7-410a-a06a-d3cd991bb746")),
        FieldType.DateTime => ("\"2026-10-23T01:02:03.1234567Z\"",
            new DateTime(2026, 10, 23, 1, 2, 3, DateTimeKind.Utc).AddTicks(1234567)),
        FieldType.DateTimeOffset => ("\"2026-10-23T01:02:03.1234567+05:45\"",
            new DateTimeOffset(2026, 10, 23, 1, 2, 3, TimeSpan.FromMinutes(345)).AddTicks(1234567)),
        FieldType.TimeSpan => ("\"-1.02:03:04.1234567\"", -new TimeSpan(1, 2, 3, 4).Add(TimeSpan.FromTicks(1234567))),
        FieldType.DateOnly => ("\"2024-02-29\"", new DateOnly(2024, 2, 29)),
        FieldType.TimeOnly => ("\"23:59:59.1234567\"", new TimeOnly(23, 59, 59).Add(TimeSpan.FromTicks(1234567))),
        FieldType.Binary => ("\"AAECA/7/\"", new byte[] { 0, 1, 2, 3, 254, 255 }),
        FieldType.Enum => ("\"paid\"", "paid"),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    private static JsonObject Definition(FieldType type)
    {
        JsonObject Field(string name, bool collection = false)
        {
            var field = new JsonObject { ["name"] = name, ["type"] = type.ToString(), ["isCollection"] = collection };
            if (type == FieldType.Enum) field["enumValues"] = new JsonArray("new", "paid");
            return field;
        }
        JsonArray Fields() => new(Field("value"), Field("values", true));
        return new JsonObject
        {
            ["formatVersion"] = "1.0",
            ["name"] = "persisted",
            ["fields"] = new JsonArray(
                Field("value"),
                Field("values", true),
                new JsonObject { ["name"] = "child", ["fields"] = Fields() },
                new JsonObject { ["name"] = "children", ["isCollection"] = true, ["fields"] = Fields() },
                Field("nullable"), Field("missing"))
        };
    }

    private static ServiceProvider Host(JsonObject definition, JsonOptions? options = null)
    {
        var services = new ServiceCollection();
        new StateflowsBuilder(services).AddEvents(events =>
        {
            JsonEventDefinition.Parse(definition.ToJsonString(), options ?? new()).Register(events);
            events.AddEvent("reply", _ => { });
        });
        return services.BuildServiceProvider();
    }
}
