using System;
using System.Linq;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Validation;

namespace Stateflows.StateMachines.Exceptions
{
    /// <summary>
    /// A state machine definition contains one or more validation errors.
    /// </summary>
    public sealed class StateMachineValidationException : BehaviorDefinitionException
    {
        /// <summary>The accumulated definition errors and warnings.</summary>
        public ModelValidationReport ValidationReport { get; }

        internal StateMachineValidationException(ModelValidationReport report, StateMachineClass stateMachineClass)
            : base(string.Join(Environment.NewLine, report.Diagnostics.Select(
                diagnostic => $"{diagnostic.RuleId} [{diagnostic.ElementPath}]: {diagnostic.Message}")),
                stateMachineClass.BehaviorClass)
        {
            ValidationReport = report;
        }
    }
}
