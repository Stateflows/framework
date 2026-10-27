using Stateflows.Common.Exceptions;
using Stateflows.Common.Validation;

namespace Stateflows.StateMachines.Json;

/// <summary>Accumulated document, binding, expansion, or graph-validation diagnostics.</summary>
public sealed class JsonStateMachineException : BehaviorDefinitionException
{
    /// <summary>Gets the diagnostics that describe the invalid or unbindable definition.</summary>
    public ModelValidationReport ValidationReport { get; }

    internal JsonStateMachineException(ModelValidationReport report, string name, Exception? inner = null)
        : base(string.Join(Environment.NewLine, report.Diagnostics.Select(
            diagnostic => $"{diagnostic.RuleId} [{diagnostic.ElementPath}]: {diagnostic.Message}")),
            new StateMachineClass(name).BehaviorClass, inner!)
        => ValidationReport = report;
}
