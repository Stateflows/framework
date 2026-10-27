using System.Collections.Generic;
using System.Linq;

namespace Stateflows.Common.Validation
{
    /// <summary>Whether a model diagnostic prevents registration.</summary>
    public enum ModelDiagnosticSeverity
    {
        /// <summary>The model remains valid.</summary>
        Warning,
        /// <summary>The model cannot be registered.</summary>
        Error
    }

    /// <summary>A diagnostic for a specific model element.</summary>
    /// <param name="RuleId">Stable validation rule identifier.</param>
    /// <param name="Severity">Whether this diagnostic prevents registration.</param>
    /// <param name="ElementPath">Graph and offending element location.</param>
    /// <param name="Message">Explanation of the problem.</param>
    public sealed record ModelDiagnostic(
        string RuleId,
        ModelDiagnosticSeverity Severity,
        string ElementPath,
        string Message);

    /// <summary>Accumulated errors and warnings from model validation.</summary>
    public sealed class ModelValidationReport
    {
        private readonly List<ModelDiagnostic> diagnostics = [];

        /// <summary>Diagnostics in validation order.</summary>
        public IReadOnlyList<ModelDiagnostic> Diagnostics => diagnostics.AsReadOnly();

        /// <summary>Whether registration must fail.</summary>
        public bool HasErrors => diagnostics.Any(d => d.Severity == ModelDiagnosticSeverity.Error);

        internal void Add(string ruleId, string path, string message,
            ModelDiagnosticSeverity severity = ModelDiagnosticSeverity.Error)
            => diagnostics.Add(new ModelDiagnostic(ruleId, severity, path, message));
    }
}
