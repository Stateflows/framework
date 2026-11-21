using Stateflows;

namespace Orleans.IntegrationTests;

internal static class StorageTestHelpers
{
    public static async Task AssertCommitFailureAsync(Func<Task> action, string injectedMessage)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            Assert.IsTrue(exception.ToString().Contains(injectedMessage, StringComparison.Ordinal),
                $"The persistence failure must preserve its cause: {exception}");
            return;
        }

        Assert.Fail("A failed state write must propagate its exception.");
    }

    public static async Task<StateflowsInfrastructureException> AssertInfrastructureFailureAsync(Func<Task> action, string injectedMessage)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            Assert.IsInstanceOfType<StateflowsInfrastructureException>(exception,
                $"Expected StateflowsInfrastructureException but got: {exception}");
            var infrastructure = (StateflowsInfrastructureException)exception;
            Assert.IsNotNull(infrastructure.InnerException);
            Assert.IsTrue(infrastructure.InnerException.ToString().Contains(injectedMessage, StringComparison.Ordinal),
                $"The infrastructure failure must preserve its cause: {infrastructure}");
            return infrastructure;
        }

        Assert.Fail("An infrastructure prerequisite failure must throw StateflowsInfrastructureException.");
        return null!;
    }
}
