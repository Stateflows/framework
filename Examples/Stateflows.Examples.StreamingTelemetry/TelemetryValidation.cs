using FluentValidation;
using Stateflows.Common;

namespace Stateflows.Examples.StreamingTelemetry;

public sealed class ConfigureTelemetryValidator : AbstractValidator<ConfigureTelemetry>
{
    public ConfigureTelemetryValidator()
    {
        RuleFor(settings => settings.Gain).InclusiveBetween(0.01m, 100m);
        RuleFor(settings => settings.Offset).InclusiveBetween(-1000m, 1000m);
        RuleFor(settings => settings.AlertThreshold).InclusiveBetween(-1000000m, 1000000m);
        RuleFor(settings => settings.ProcessingDelayMilliseconds).InclusiveBetween(0, 5000);
    }
}

public sealed class SensorBatchValidator : AbstractValidator<TokensInput<SensorSample>>
{
    public SensorBatchValidator()
    {
        RuleFor(input => input.Tokens).NotEmpty().Must(tokens => tokens is null || tokens.Count <= 64)
            .WithMessage("Submit between 1 and 64 samples per batch.");
    }
}
