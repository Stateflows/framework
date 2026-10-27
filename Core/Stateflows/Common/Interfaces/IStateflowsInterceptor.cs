namespace Stateflows.Common.Interfaces;

/// <summary>Combines behavior, execution, and tenant-level interception contracts.</summary>
public interface IStateflowsInterceptor : IBehaviorInterceptor, IStateflowsExecutionInterceptor, IStateflowsTenantInterceptor;