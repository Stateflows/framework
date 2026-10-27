using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.Common.Registration.Builders;
using Stateflows.Events;
using Stateflows.Events.Json;

namespace StateMachine.IntegrationTests.Tests.Json;

[TestClass]
[TestCategory("JsonStructuralEvents")]
public class JsonStructuralEvents
{
    private const string Definition = """
        {
          "formatVersion":"1.0", "name":"order",
          "fields":[
            {"name":"id","type":"Int32","validation":[{"type":"Required"}]},
            {"name":"total","type":"Decimal","validation":[
              {"type":"Range","options":{"operandType":"Decimal","minimum":0,"maximum":100}}
            ]},
            {"name":"status","type":"Enum","enumValues":["new","paid"]},
            {"name":"tags","type":"String","isCollection":true,
             "itemValidation":[{"type":"StringLength","options":{"maximumLength":3}}]},
            {"name":"address","fields":[{"name":"city","type":"String"}]},
            {"name":"lines","isCollection":true,"fields":[{"name":"count","type":"Int64"}]},
            {"name":"optional","type":"Boolean"}
          ]
        }
        """;

    private static ServiceProvider Host(string json = Definition, JsonOptions? options = null)
    {
        var services = new ServiceCollection();
        var builder = new StateflowsBuilder(services);
        builder.AddEvents(events => JsonEventDefinition.Parse(json, options ?? new()).Register(events));
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public void PayloadsUseDeclaredClrTypesAndDiscardUnknownFieldsRecursively()
    {
        using var host = Host();
        var value = host.GetRequiredService<IEventFactory>().CreateFromJson("order", """
            {"id":42,"total":12.50,"status":"new","tags":["a",null],
             "address":{"city":"Paris","ignored":{"untyped":true}},
             "lines":[{"count":9223372036854775807,"ignored":1},null],
             "unknown":1,"optional":null}
            """);
        Assert.AreEqual(42, value.Get<int>("id"));
        Assert.AreEqual(12.50m, value.Get<decimal>("total"));
        Assert.IsFalse(value.ContainsField("unknown"));
        Assert.IsNull(value.Get<object>("optional"));
        Assert.IsFalse(value.Get<StructuralValue>("address").ContainsField("ignored"));
        Assert.AreEqual(long.MaxValue, value.GetCollection<StructuralValue>("lines")[0].Get<long>("count"));
        Assert.IsNull(value.GetCollection<StructuralValue>("lines")[1]);
        Assert.IsNull(value.GetCollection<string>("tags")[1]);
        var missing = host.GetRequiredService<IEventFactory>().CreateFromJson("order", "{}");
        Assert.AreEqual(0, missing.Fields.Count);
        Assert.IsFalse(missing.ContainsField("id"));
        Assert.IsFalse(host.GetRequiredService<IEventsRegister>().GetEvent("order").Fields["id"].Validation[0].IsValid(null));
    }

    [TestMethod]
    [DataRow("""{"id":"1"}""")]
    [DataRow("""{"id":true}""")]
    [DataRow("""{"id":2147483648}""")]
    [DataRow("""{"id":1.1}""")]
    [DataRow("""{"total":"1.5"}""")]
    [DataRow("""{"total":79228162514264337593543950336}""")]
    [DataRow("""{"optional":"true"}""")]
    [DataRow("""{"status":"NEW"}""")]
    [DataRow("""{"address":[]}""")]
    [DataRow("""{"lines":[1]}""")]
    [DataRow("""{"tags":[false]}""")]
    [DataRow("""{"tags":"x"}""")]
    [DataRow("""{"id":1,"id":2}""")]
    [DataRow("""{"unknown":{"x":1,"x":2}}""")]
    [DataRow("""[]""")]
    [DataRow("""{} {}""")]
    [DataRow("""{"id":1,}""")]
    public void RejectsMalformedOrCoercedPayloads(string json)
    {
        using var host = Host();
        Assert.Throws<JsonException>(() => host.GetRequiredService<IEventFactory>().CreateFromJson("order", json));
    }

    [TestMethod]
    [DataRow("""{"name":"x","fields":[]}""")]
    [DataRow("""{"formatVersion":"2.0","name":"x","fields":[]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[],"typo":1}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"int32"}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"0"}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":0}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"Enum"}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"String","fields":[]}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"String","name":"b"}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"String"},{"name":"a","type":"String"}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a"}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[null]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[],"response":{"clrType":"System.String"}}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[],"response":{"clrType":"s","eventName":"y"}}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"String","validation":[{"type":"Custom","alias":"missing"}]}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"String","validation":[{"type":"Required","options":{"typo":true}}]}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"String","validation":[{"type":"Unknown"}]}]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"x","fields":[{"name":"a","type":"String","validation":[{"type":"Compare","options":{"otherProperty":"missing"}}]}]}""")]
    public void RejectsMalformedOrUnknownDefinitions(string json)
        => Assert.Throws<JsonException>(() => JsonEventDefinition.Parse(json, new()));

    [TestMethod]
    public void ScalarFormatsAreDecodedInvariantly()
    {
        var types = new[] { "Guid", "DateTime", "DateTimeOffset", "TimeSpan", "DateOnly", "TimeOnly", "Binary", "Double", "Boolean" };
        var fields = types.Select(t => new { name = t, type = t }).ToArray();
        var definition = JsonSerializer.Serialize(new { formatVersion = "1.0", name = "formats", fields });
        using var host = Host(definition);
        var value = host.GetRequiredService<IEventFactory>().CreateFromJson("formats", """
            {"Guid":"d4619b50-34f7-410a-a06a-d3cd991bb746","DateTime":"2026-10-23T01:02:03Z",
             "DateTimeOffset":"2026-10-23T01:02:03+02:00","TimeSpan":"1.02:03:04",
             "DateOnly":"2026-10-23","TimeOnly":"12:34:56.1234567","Binary":"AQID","Double":1.25,"Boolean":true}
            """);
        Assert.IsInstanceOfType<Guid>(value.Fields["Guid"]);
        Assert.IsInstanceOfType<DateTime>(value.Fields["DateTime"]);
        Assert.AreEqual(TimeSpan.FromHours(2), value.Get<DateTimeOffset>("DateTimeOffset").Offset);
        Assert.AreEqual(new TimeSpan(1, 2, 3, 4), value.Get<TimeSpan>("TimeSpan"));
        Assert.AreEqual(new DateOnly(2026, 10, 23), value.Get<DateOnly>("DateOnly"));
        Assert.AreEqual(1234567L, value.Get<TimeOnly>("TimeOnly").Ticks % TimeSpan.TicksPerSecond);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, value.Get<byte[]>("Binary"));
        Assert.AreEqual(1.25d, value.Get<double>("Double"));
        Assert.IsTrue(value.Get<bool>("Boolean"));
        foreach (var (type, invalid) in new[] { ("Guid", "\"bad\""), ("DateOnly", "\"10/23/2026\""),
            ("DateTime", "\"tomorrow\""), ("DateTimeOffset", "\"bad\""), ("TimeOnly", "\"1 PM\""),
            ("TimeSpan", "\"PT1H\""), ("Binary", "\"AQ ID\""), ("Double", "1e400") })
            Assert.Throws<JsonException>(() => host.GetRequiredService<IEventFactory>().CreateFromJson("formats", $"{{\"{type}\":{invalid}}}"));
    }

    [TestMethod]
    public void RequestResponsesAndCustomValidatorsUseHostAliasesOnly()
    {
        var definition = JsonNode.Parse(Definition)!;
        definition["response"] = new JsonObject { ["clrType"] = "response" };
        definition["fields"]![0]!["validation"] = JsonNode.Parse("""
            [{"type":"Custom","alias":"positive","options":{"minimum":1},"errorMessage":"Must be positive."}]
            """);
        var options = new JsonOptions().AddType<string>("response").AddValidator("positive",
            configuration => new RangeAttribute(configuration.GetProperty("minimum").GetInt32(), int.MaxValue));
        using var host = Host(definition.ToJsonString(), options);
        var request = host.GetRequiredService<IEventFactory>().CreateFromJson("order", "{}");
        Assert.IsInstanceOfType<StructuralRequest>(request);
        var registered = host.GetRequiredService<IEventsRegister>().GetEvent("order");
        Assert.AreEqual(typeof(string), registered.ResponseType);
        Assert.IsFalse(registered.Fields["id"].Validation[0].IsValid(0));
        Assert.AreEqual("Must be positive.", registered.Fields["id"].Validation[0].ErrorMessage);
        Assert.IsFalse(registered.Fields["total"].Validation[0].IsValid(-1m));
        Assert.IsFalse(registered.Fields["tags"].ItemValidation[0].IsValid("long"));

        definition["response"] = new JsonObject { ["eventName"] = "reply" };
        using var structuralHost = Host(definition.ToJsonString(), options);
        var structural = structuralHost.GetRequiredService<IEventFactory>().CreateFromJson("order", "{}");
        Assert.AreEqual("reply", ((StructuralRequest)structural).ResponseEventName);
    }
}
