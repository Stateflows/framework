using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Context;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.Common.Engine
{
    internal sealed class ContextExecution
    {
        public StateflowsContext Context { get; set; }
        public bool Persist { get; set; } = true;

        private ContextExecution(StateflowsContext context)
        {
            Context = context;
        }

        public static async Task<EventStatus> RunAsync(
            IStateflowsStorage storage,
            BehaviorId id,
            List<Exception> exceptions,
            Func<ContextExecution, Task<EventStatus>> executeAsync)
        {
            var exceptionCount = exceptions.Count;
            ContextExecution? execution = null;
            using var pendingTasks = new PendingTasksScope();
            using var publications = PublicationScope.IsRuntimeOwned ? null : new PublicationScope();
            try
            {
                var stored = await storage.HydrateAsync(id);
                // Runtime-backed storage can return its committed object, not a detached copy.
                var working = StateflowsJsonConverter.Clone(stored);
                working.Stored = stored.Stored;
                working.Deleted = stored.Deleted;
                execution = new ContextExecution(working);
                var result = await executeAsync(execution);

                await pendingTasks.Tasks.DrainAsync();
                pendingTasks.Tasks.ThrowIfFailed();

                if (publications != null)
                    await publications.WaitForImmediateAsync();

                if (result == EventStatus.Failed ||
                    exceptions.Count != exceptionCount ||
                    !execution.Context.ExecutionExceptions.IsEmpty)
                {
                    return EventStatus.Failed;
                }

                if (execution.Persist && result != EventStatus.Undelivered)
                {
                    execution.Context.LastExecutedAt = DateTime.Now;
                    await storage.DehydrateAsync(execution.Context);
                }

                if (publications != null)
                    await publications.FlushAsync();

                return result;
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
                return EventStatus.Failed;
            }
            finally
            {
                await pendingTasks.Tasks.DrainAsync();
                foreach (var exception in pendingTasks.Tasks.Exceptions)
                {
                    if (!exceptions.Any(existing => ReferenceEquals(existing, exception)))
                        exceptions.Add(exception);
                }
                if (publications != null)
                {
                    try
                    {
                        await publications.WaitForImmediateAsync();
                    }
                    catch (Exception exception)
                    {
                        if (!exceptions.Any(existing => ReferenceEquals(existing, exception)))
                            exceptions.Add(exception);
                    }
                }
                if (execution != null)
                {
                    foreach (var exception in execution.Context.ExecutionExceptions)
                    {
                        if (!exceptions.Any(existing => ReferenceEquals(existing, exception)))
                        {
                            exceptions.Add(exception);
                        }
                    }
                }
            }
        }
    }
}
