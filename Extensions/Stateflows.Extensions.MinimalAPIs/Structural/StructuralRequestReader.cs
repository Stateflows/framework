using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Stateflows.Common;
using Stateflows.Events.Json;

namespace Stateflows.Extensions.MinimalAPIs;

internal sealed class StructuralRequestPayload
{
    public StructuralEvent? Event { get; init; }
    public string[] RequestedNotifications { get; init; } = [];
    public List<ValidationResult> Errors { get; } = [];
}

/// <summary>
/// Reads Minimal APIs request envelope (<c>{ "event": { ... }, "requestedNotifications": [ ... ] }</c>) for structural events.
/// Event payload is decoded by the canonical Stateflows JSON structural decoder (<see cref="JsonEventFactoryExtensions"/>).
/// </summary>
internal static class StructuralRequestReader
{
    public static async Task<StructuralRequestPayload> ReadAsync(HttpRequest request, EventDefinition definition,
        IEventFactory factory, IEventsRegister register, CancellationToken cancellationToken)
    {
        JsonDocument? document = null;
        try
        {
            if (request.ContentLength is not 0)
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(body))
                {
                    document = JsonDocument.Parse(body);
                }
            }
        }
        catch (JsonException e)
        {
            var failed = new StructuralRequestPayload();
            failed.Errors.Add(new ValidationResult($"Request body is not a valid JSON: {e.Message}"));
            return failed;
        }

        using (document)
        {
            return Read(document?.RootElement, definition, factory, register);
        }
    }

    internal static StructuralRequestPayload Read(JsonElement? root, EventDefinition definition, IEventFactory factory, IEventsRegister register)
    {
        string[] notifications = [];
        JsonElement? eventElement = null;
        var errors = new List<ValidationResult>();

        if (root is { ValueKind: JsonValueKind.Object } rootObject)
        {
            foreach (var property in rootObject.EnumerateObject())
            {
                if (string.Equals(property.Name, "event", StringComparison.OrdinalIgnoreCase))
                {
                    eventElement = property.Value;
                }
                else if (string.Equals(property.Name, "requestedNotifications", StringComparison.OrdinalIgnoreCase))
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        notifications = property.Value
                            .EnumerateArray()
                            .Where(e => e.ValueKind == JsonValueKind.String)
                            .Select(e => e.GetString()!)
                            .Where(n => !string.IsNullOrEmpty(n))
                            .ToArray();
                    }
                    else if (property.Value.ValueKind != JsonValueKind.Null)
                    {
                        errors.Add(new ValidationResult("'requestedNotifications' must be an array of strings.", ["requestedNotifications"]));
                    }
                }
            }
        }
        else if (root is { ValueKind: not JsonValueKind.Null })
        {
            errors.Add(new ValidationResult("Request body must be a JSON object."));
        }

        string? eventJson = null;
        if (eventElement is { ValueKind: JsonValueKind.Object } eventObject)
        {
            eventJson = eventObject.GetRawText();
        }
        else if (eventElement is { ValueKind: not JsonValueKind.Null })
        {
            errors.Add(new ValidationResult("'event' must be a JSON object.", ["event"]));
        }
        else if (definition.Fields.Count == 0)
        {
            // Events without fields do not require event payload
            eventJson = "{}";
        }
        else
        {
            errors.Add(new ValidationResult("Event not provided"));
        }

        StructuralEvent? @event = null;
        if (errors.Count == 0 && eventJson != null)
        {
            try
            {
                @event = factory.CreateFromJson(definition.Name, eventJson, register);
            }
            catch (Exception e) when (e is JsonException or ArgumentException or InvalidOperationException)
            {
                errors.Add(new ValidationResult(e.Message, ["event"]));
            }
        }

        var payload = new StructuralRequestPayload
        {
            Event = @event,
            RequestedNotifications = notifications
        };
        payload.Errors.AddRange(errors);

        return payload;
    }
}