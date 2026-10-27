using System.Text.Json.Nodes;
using Json.Schema;
using Stateflows.Common;
using Stateflows.Common.Json;
using Stateflows.Events.Json;

namespace StateMachine.IntegrationTests.Tests.Json;

[TestClass]
public class JsonStructuralEventSchema
{
    private static readonly JsonSchema Schema = LoadSchema();

    private static JsonSchema LoadSchema()
    {
        using var stream = typeof(JsonEventDefinition).Assembly.GetManifestResourceStream(
            "Stateflows.Extensions.Json.event.schema.json");
        Assert.IsNotNull(stream, "The event schema must be embedded in the JSON extension.");
        using var reader = new StreamReader(stream);
        return JsonSchema.FromText(reader.ReadToEnd());
    }

    private static bool IsValid(JsonNode document)
        => Schema.Evaluate(document, new EvaluationOptions { OutputFormat = OutputFormat.List }).IsValid;

    private static JsonObject Document(JsonNode? field = null) => new()
    {
        ["formatVersion"] = "1.0",
        ["name"] = "order",
        ["fields"] = field == null ? new JsonArray() : new JsonArray(field)
    };

    private static JsonObject Field(string validation) => new()
    {
        ["name"] = "value",
        ["type"] = "String",
        ["validation"] = new JsonArray(JsonNode.Parse(validation))
    };

    [TestMethod]
    public void SchemaIsCopiedToOutputAndHasVersionedIdentity()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "event.schema.json");
        Assert.IsTrue(File.Exists(path));
        var schema = JsonNode.Parse(File.ReadAllText(path))!;
        Assert.AreEqual("urn:stateflows:event:1.0", schema["$id"]!.GetValue<string>());
        Assert.AreEqual("https://json-schema.org/draft/2020-12/schema", schema["$schema"]!.GetValue<string>());
    }

    [TestMethod]
    public void AcceptsEveryScalarTypeAndScalarCollection()
    {
        foreach (var type in Enum.GetValues<FieldType>())
        {
            var field = new JsonObject { ["name"] = "value", ["type"] = type.ToString() };
            if (type == FieldType.Enum) field["enumValues"] = new JsonArray("new", "paid");
            var document = Document(field);
            Assert.IsTrue(IsValid(document), type.ToString());
            JsonEventDefinition.Parse(document.ToJsonString(), new JsonOptions());
            field["isCollection"] = true;
            field["itemValidation"] = new JsonArray(new JsonObject { ["type"] = "Required" });
            Assert.IsTrue(IsValid(document), $"{type} collection");
            JsonEventDefinition.Parse(document.ToJsonString(), new JsonOptions());
        }
    }

    [TestMethod]
    public void AcceptsRecursiveObjectsAndObjectCollections()
    {
        var document = Document(JsonNode.Parse("""
            {"name":"items","isCollection":true,"validation":[{"type":"Required"}],
             "fields":[{"name":"address","type":null,"fields":[{"name":"city","type":"String"}]}]}
            """));
        Assert.IsTrue(IsValid(document));
        JsonEventDefinition.Parse(document.ToJsonString(), new JsonOptions());
    }

    [TestMethod]
    [DataRow("""{"eventName":"receipt"}""")]
    [DataRow("""{"clrType":"responses.receipt"}""")]
    [DataRow("""{"eventName":"receipt","clrType":null}""")]
    [DataRow("""{"clrType":"responses.receipt","eventName":null}""")]
    [DataRow("null")]
    public void AcceptsResponseReferences(string response)
    {
        var document = Document();
        document["response"] = JsonNode.Parse(response);
        Assert.IsTrue(IsValid(document));
        JsonEventDefinition.Parse(document.ToJsonString(), new JsonOptions().AddType<string>("responses.receipt"));
    }

    [TestMethod]
    [DataRow("""{}""")]
    [DataRow("""{"eventName":"receipt","clrType":"responses.receipt"}""")]
    [DataRow("""{"eventName":" "}""")]
    [DataRow("""{"eventName":null,"clrType":null}""")]
    [DataRow("""{"eventName":"receipt","extra":true}""")]
    public void RejectsInvalidResponseReferences(string response)
    {
        var document = Document();
        document["response"] = JsonNode.Parse(response);
        Assert.IsFalse(IsValid(document));
    }

    [TestMethod]
    [DataRow("""{"type":"Required"}""")]
    [DataRow("""{"type":"Required","options":{"allowEmptyStrings":true}}""")]
    [DataRow("""{"type":"StringLength","options":{"maximumLength":20,"minimumLength":1}}""")]
    [DataRow("""{"type":"MinLength","options":{"length":0}}""")]
    [DataRow("""{"type":"MaxLength","options":{"length":20}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Int32","minimum":-2147483648,"maximum":2147483647}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Int64","minimum":-9223372036854775808,"maximum":9223372036854775807}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Decimal","minimum":0.5,"maximum":1.5}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Double","minimum":0,"maximum":1e20}}""")]
    [DataRow("""{"type":"RegularExpression","options":{"pattern":"^[a-z]+$","matchTimeoutInMilliseconds":10}}""")]
    [DataRow("""{"type":"Compare","options":{"otherProperty":"value"}}""")]
    [DataRow("""{"type":"EmailAddress"}""")]
    [DataRow("""{"type":"Url","options":{}}""")]
    [DataRow("""{"type":"Phone","errorMessage":"Invalid phone"}""")]
    [DataRow("""{"type":"CreditCard","errorMessage":null,"alias":null}""")]
    [DataRow("""{"type":"Custom","alias":"host.rule","options":{"hostOwned":[1,true]}}""")]
    public void AcceptsSupportedValidationOptions(string validation)
    {
        var document = Document(Field(validation));
        Assert.IsTrue(IsValid(document), validation);
        JsonEventDefinition.Parse(document.ToJsonString(), new JsonOptions().AddValidator(
            "host.rule", _ => new System.ComponentModel.DataAnnotations.RequiredAttribute()));
    }

    [TestMethod]
    [DataRow("""{"type":"Required","alias":"host.rule"}""")]
    [DataRow("""{"type":"Required","options":{"typo":true}}""")]
    [DataRow("""{"type":"Required","options":null}""")]
    [DataRow("""{"type":"StringLength"}""")]
    [DataRow("""{"type":"StringLength","options":{"maximumLength":-1}}""")]
    [DataRow("""{"type":"MinLength","options":{}}""")]
    [DataRow("""{"type":"MaxLength","options":{"length":2147483648}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"String","minimum":0,"maximum":1}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Int32","minimum":0,"maximum":2147483648}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Int64","minimum":0.5,"maximum":10}}""")]
    [DataRow("""{"type":"Range","options":{"operandType":"Decimal","minimum":"0","maximum":1}}""")]
    [DataRow("""{"type":"RegularExpression","options":{"pattern":""}}""")]
    [DataRow("""{"type":"RegularExpression","options":{"pattern":"x","matchTimeoutInMilliseconds":0}}""")]
    [DataRow("""{"type":"Compare","options":{"otherProperty":" "}}""")]
    [DataRow("""{"type":"EmailAddress","options":{"extra":true}}""")]
    [DataRow("""{"type":"Custom"}""")]
    [DataRow("""{"type":"Custom","alias":null}""")]
    [DataRow("""{"type":"Unknown"}""")]
    [DataRow("""{"type":"Required","extra":true}""")]
    public void RejectsInvalidValidationOptions(string validation)
        => Assert.IsFalse(IsValid(Document(Field(validation))), validation);

    [TestMethod]
    [DataRow("""{"name":"value","type":"String","fields":[]}""")]
    [DataRow("""{"name":"value","type":"String","itemValidation":[]}""")]
    [DataRow("""{"name":"value","fields":[],"itemValidation":[]}""")]
    [DataRow("""{"name":"value","isCollection":true,"fields":[],"itemValidation":[]}""")]
    [DataRow("""{"name":"value","type":"String","enumValues":["x"]}""")]
    [DataRow("""{"name":"value","type":"Enum"}""")]
    [DataRow("""{"name":"value","type":"Enum","enumValues":[]}""")]
    [DataRow("""{"name":"value","type":"Enum","enumValues":["x","x"]}""")]
    [DataRow("""{"name":"value","type":"Enum","enumValues":[" "]}""")]
    [DataRow("""{"name":"value"}""")]
    [DataRow("""{"name":"value","type":null,"fields":null}""")]
    [DataRow("""{"name":"value","type":"int32"}""")]
    [DataRow("""{"name":"value","type":0}""")]
    [DataRow("""{"name":" ","type":"String"}""")]
    [DataRow("""{"name":"value","type":"String","extra":true}""")]
    public void RejectsInvalidFieldShapes(string field)
        => Assert.IsFalse(IsValid(Document(JsonNode.Parse(field))), field);

    [TestMethod]
    [DataRow("""{"name":"value","type":"String","fields":null,"enumValues":null,"validation":null,"itemValidation":null}""")]
    [DataRow("""{"name":"value","fields":[]}""")]
    [DataRow("""{"name":"value","type":null,"fields":[],"itemValidation":null}""")]
    public void AcceptsParserSupportedNullAndEmptyDefinitions(string field)
    {
        var document = Document(JsonNode.Parse(field));
        Assert.IsTrue(IsValid(document));
        JsonEventDefinition.Parse(document.ToJsonString(), new JsonOptions());
    }

    [TestMethod]
    [DataRow("""{}""")]
    [DataRow("""{"formatVersion":"2.0","name":"order","fields":[]}""")]
    [DataRow("""{"formatVersion":"1.0","name":" ","fields":[]}""")]
    [DataRow("""{"formatVersion":"1.0","name":"order","fields":null}""")]
    [DataRow("""{"formatVersion":"1.0","name":"order","fields":[],"extra":true}""")]
    public void RejectsInvalidDocuments(string json)
        => Assert.IsFalse(IsValid(JsonNode.Parse(json)!));
}
