using Orleans;
using Orleans.Runtime;
using Orleans.Transactions;

namespace Stateflows;

/// <summary>
/// An Orleans runtime prerequisite failed. The underlying infrastructure error is retained as the inner exception.
/// </summary>
[GenerateSerializer]
public sealed class StateflowsInfrastructureException(string message, Exception innerException)
    : OrleansException(message, innerException)
{
    internal static Exception? GetFailure(Exception? exception)
    {
        while (exception is OrleansTransactionAbortedException && exception.InnerException != null)
            exception = exception.InnerException;
        return exception switch
        {
            StateflowsInfrastructureException failure => failure,
            OrleansTransactionException failure when failure is not OrleansTransactionAbortedException => failure,
            _ => null
        };
    }
}
