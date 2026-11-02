namespace Stateflows.Examples.StreamingTelemetry;

public sealed record SensorSample(string SampleId, string SensorId, decimal Value, DateTimeOffset ObservedAt);

public sealed record ConfigureTelemetry(
    decimal Gain, decimal Offset, decimal AlertThreshold, int ProcessingDelayMilliseconds)
{
    public static ConfigureTelemetry Default { get; } = new(1m, 0m, 50m, 1500);
}

public sealed record ProcessedReading(
    string SampleId, string SensorId, decimal RawValue, decimal CalibratedValue,
    DateTimeOffset ObservedAt, DateTimeOffset ProcessedAt, decimal AlertThreshold,
    string ExecutionMode, Guid? ExecutionTriggerId);

public sealed record TelemetryAlert(
    string SampleId, string SensorId, decimal CalibratedValue, decimal Threshold, DateTimeOffset ObservedAt);

public sealed record RejectedSample(string? SampleId, string? SensorId, string Reason);

public sealed record TelemetryConfigured(ConfigureTelemetry Settings);
