using Stateflows.Common.Validation;

namespace Stateflows.Activities.Json;

/// <summary>Activity JSON parsing, binding, or graph-validation failure.</summary>
public sealed class JsonActivityException : Exception
{
    public ModelValidationReport ValidationReport { get; }
    public string ActivityName { get; }

    internal JsonActivityException(ModelValidationReport report, string activityName, Exception? inner = null)
        : base($"Activity JSON definition '{activityName}' is invalid:{Environment.NewLine}{string.Join(Environment.NewLine,
            report.Diagnostics.Select(diagnostic => $"{diagnostic.RuleId} {diagnostic.ElementPath}: {diagnostic.Message}"))}", inner)
    {
        ValidationReport = report;
        ActivityName = activityName;
    }
}
