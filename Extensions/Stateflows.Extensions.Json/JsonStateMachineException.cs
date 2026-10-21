using Stateflows.Common.Exceptions;
using Stateflows.Common.Validation;

namespace Stateflows.StateMachines.Json;

/// <summary>Accumulated document, binding, expansion, or graph-validation diagnostics.</summary>
public sealed class JsonStateMachineException : BehaviorDefinitionException
{
    public ModelValidationReport ValidationReport { get; }

    internal JsonStateMachineException(ModelValidationReport report, string name, Exception? inner = null)
        : base(string.Join(Environment.NewLine, report.Diagnostics.Select(
            diagnostic => $"{diagnostic.RuleId} [{diagnostic.ElementPath}]: {diagnostic.Message}")),
            new StateMachineClass(name).BehaviorClass, inner!)
        => ValidationReport = report;
}
