using Stateflows.Activities;
using Stateflows.Extensions.MinimalAPIs;

namespace Stateflows.Examples.StreamingTelemetry;

public sealed class TelemetryActivity : IActivity
{
    public const string Name = "Telemetry";
    public static void Build(IActivityBuilder activity) => activity
        .AddEndpoints(endpoints => endpoints
            .AddGet("/diagram", TelemetryEndpoints.DiagramAsync))
        .AddDefaultInitializer<TelemetryInitializer>()
        .AddAcceptEventAction<ConfigureTelemetry, ConfigureTelemetryAction>()
        .AddInputPin<SensorSample>(pin => pin
            .AddFlow<ValidateSamplesAction>())
        .AddAction<ValidateSamplesAction>(node => node
            .AddFlow<SensorSample, CalibrateSamplesAction>()
            .AddFlow<RejectedSample, OutputPin<RejectedSample>>())
        .AddAction<CalibrateSamplesAction>(node => node
            .Reentrant()
            .AddFlow<ProcessedReading, ForkNode>())
        .AddFork(fork => fork
            .AddFlow<ProcessedReading, OutputPin<ProcessedReading>>()
            .AddFlow<ProcessedReading, DetectAlertsAction>())
        .AddAction<DetectAlertsAction>(node => node
            .AddFlow<TelemetryAlert, OutputPin<TelemetryAlert>>())
        .AddOutputPin<ProcessedReading>()
        .AddOutputPin<TelemetryAlert>()
        .AddOutputPin<RejectedSample>();
}
