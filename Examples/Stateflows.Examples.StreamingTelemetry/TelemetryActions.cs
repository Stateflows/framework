using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Examples.StreamingTelemetry;

internal static class TelemetryValues
{
    internal const string SettingsKey = "telemetry-settings";
}

public sealed class TelemetryInitializer(IActivityContext activity) : IDefaultInitializer
{
    public async Task<bool> OnInitializeAsync()
    {
        await activity.Values.SetAsync(TelemetryValues.SettingsKey, ConfigureTelemetry.Default);
        return true;
    }
}

public sealed class ConfigureTelemetryAction(IActivityContext activity) : IAcceptEventActionNode<ConfigureTelemetry>
{
    public async Task ExecuteAsync(ConfigureTelemetry @event, CancellationToken cancellationToken)
    {
        await activity.Values.SetAsync(TelemetryValues.SettingsKey, @event);
        activity.Publish(new TelemetryConfigured(@event));
    }
}

public sealed class ValidateSamplesAction(
    IInputTokens<SensorSample> samples,
    IOutputTokens<SensorSample> validSamples,
    IOutputTokens<RejectedSample> rejectedSamples) : IActionNode
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var sample in samples)
        {
            var reason = sample switch
            {
                null => "A sample cannot be null.",
                { SampleId: null or "" } => "SampleId is required.",
                { SensorId: null or "" } => "SensorId is required.",
                { Value: < -10000m or > 10000m } => "Value must be between -10000 and 10000.",
                _ when string.IsNullOrWhiteSpace(sample.SampleId) || sample.SampleId.Length > 100
                    => "SampleId must contain 1 to 100 non-whitespace characters.",
                _ when string.IsNullOrWhiteSpace(sample.SensorId) || sample.SensorId.Length > 100
                    => "SensorId must contain 1 to 100 non-whitespace characters.",
                _ when sample.ObservedAt == default => "ObservedAt is required.",
                _ => null
            };
            if (reason is null)
                validSamples.Add(sample);
            else
                rejectedSamples.Add(new RejectedSample(sample?.SampleId, sample?.SensorId, reason));
        }
        return Task.CompletedTask;
    }
}

public sealed class CalibrateSamplesAction(
    IActivityContext activity, IExecutionContext execution,
    IInputTokens<SensorSample> samples, IOutputTokens<ProcessedReading> readings) : IActionNode
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var beforeDelay = await activity.Values.GetOrDefaultAsync(TelemetryValues.SettingsKey, ConfigureTelemetry.Default);
        await Task.Delay(beforeDelay.ProcessingDelayMilliseconds, cancellationToken);
        // Read after the simulated I/O: ordinary configuration events can steer already admitted work.
        var settings = await activity.Values.GetOrDefaultAsync(TelemetryValues.SettingsKey, ConfigureTelemetry.Default);
        foreach (var sample in samples)
            readings.Add(new ProcessedReading(
                sample.SampleId, sample.SensorId, sample.Value, sample.Value * settings.Gain + settings.Offset,
                sample.ObservedAt, DateTimeOffset.UtcNow, settings.AlertThreshold,
                execution.ExecutionMode.ToString(), execution.ExecutionTriggerId));
    }
}

public sealed class DetectAlertsAction(
    IInputTokens<ProcessedReading> readings, IOutputTokens<TelemetryAlert> alerts) : IActionNode
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var reading in readings)
            if (reading.CalibratedValue >= reading.AlertThreshold)
                alerts.Add(new TelemetryAlert(
                    reading.SampleId, reading.SensorId, reading.CalibratedValue, reading.AlertThreshold, reading.ObservedAt));
        return Task.CompletedTask;
    }
}
