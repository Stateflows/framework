namespace Stateflows.Common.Registration.Interfaces;

public interface IResourceBuilder
{
    IResourceBuilder SetMaxConcurrentBehaviorExecutions(int maxConcurrentBehaviorExecutions);
}