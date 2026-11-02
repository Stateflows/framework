using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Activities.Registration;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Streams;
using System.Collections.Concurrent;
using System.Threading;
using Stateflows.Common.Context;
using Stateflows.Common.Engine;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities.Engine
{
    internal class Processor(
        ActivitiesRegister register,
        IEnumerable<IActivityEventHandler> eventHandlers,
        IStateflowsStorage storage,
        IStateflowsValueStorage valueStorage,
        IServiceProvider provider
    ) : IEventProcessor, IStateflowsProcessor, IActivityStreamAdmissionPolicy, IActivityStreamSessionProcessor
    {
        private readonly ConcurrentDictionary<BehaviorId, ActivitySession> activitySessions = new();
        private readonly ConcurrentDictionary<BehaviorId, SemaphoreSlim> sessionCreationGates = new();

        public string[] BehaviorTypes => [BehaviorType.Activity];

        public bool CanAdmitStream(BehaviorId id, StateflowsContext context)
        {
            if (activitySessions.TryGetValue(id, out var session) &&
                (!session.InitializationCompletion.Task.IsCompleted || session.Executor.Initialized))
                return true;

            var key = context.Version != 0 ? $"{id.Name}.{context.Version}" : $"{id.Name}.current";
            return register.Activities.TryGetValue(key, out var graph) &&
                (context.Status == BehaviorStatus.Initialized || !graph.InitializerTypes.Any());
        }

        public bool AcceptsInput(BehaviorId id, StateflowsContext context, TokensInputEvent input)
        {
            var graph = activitySessions.TryGetValue(id, out var session) ? session.Executor.Graph
                : register.Activities.GetValueOrDefault(context.Version != 0 ? $"{id.Name}.{context.Version}" : $"{id.Name}.current");
            return graph != null && graph.AcceptsInput(input.Tokens);
        }

        public Task<EventStatus> ProcessEventAsync<TEvent>(
            BehaviorId id,
            EventHolder<TEvent> eventHolder,
            List<Exception> exceptions)
        {
            if (eventHolder.Headers.Values.Any(header => header is Stateflows.Common.Stream or Checkpoint))
            {
                return Task.FromResult(EventStatus.Rejected);
            }

            if (eventHolder.Headers.Values.Any(header => header is StreamedActivityExecution or ActivitySessionExecution))
            {
                // Provenance classifies the source of the event, not its delivery: events delivered by the live stream
                // pump with a transactional source (live time events) run as regular transactions.
                var streamed = eventHolder.Headers.Values.OfType<StreamedActivityExecution>().Any(header => !header.TransactionalSource);
                return ExecuteInActivitySessionAsync(id, eventHolder, streamed, exceptions);
            }

            if (eventHolder.Payload is ActivityCheckpoint)
                return Task.FromResult(EventStatus.Rejected);

            return ProcessRegularEventAsync(id, eventHolder, exceptions);
        }

        public async Task<EventStatus> ExecuteInActivitySessionAsync(
            BehaviorId id,
            EventHolder eventHolder,
            bool streamed,
            List<Exception> exceptions)
        {
            var session = await GetOrCreateSessionAsync(id, streamed);
            if (session == null)
                return streamed ? EventStatus.Failed : EventStatus.Undelivered;

            using var inspection = session.Executor.BindInspection();
            if (eventHolder.BoxedPayload is TokensInputEvent input && !session.Executor.Graph.AcceptsInput(input.Tokens))
                return EventStatus.Rejected;

            if (streamed)
                session.BeginStreaming();
            var wasInitialized = session.Executor.Initialized;
            if (session.Executor.Initialized)
            {
                session.Context.IsStreamingExecution = streamed;
            }

            if (!streamed &&
                eventHolder.Headers.Values.FirstOrDefault(h => h is BehaviorEmbedding) is BehaviorEmbedding embedding)
            {
                session.Context.Context.ContextOwnerId = embedding.OwnerId;
                session.Context.Context.ContextParentId = embedding.ParentId;
            }

            var exceptionCount = exceptions.Count;
            using var pendingTasks = new PendingTasksScope();
            using var executionTrigger = session.Context.Context.PushExecutionTriggerHolder(eventHolder);
            using var invocationProvenance = session.Context.BeginInvocationProvenance(streamed, trackTransaction: true, callRoot: true);
            try
            {
                EventStatus result;
                try
                {
                    result = await ExecuteTypedSessionEventAsync(session, eventHolder, !streamed);
                }
                catch (ActivityCheckpointPauseException)
                {
                    result = EventStatus.Consumed;
                }
                catch (Exception exception) when (!streamed)
                {
                    exceptions.Add(exception);
                    result = EventStatus.Failed;
                }
                await pendingTasks.Tasks.DrainAsync();
                foreach (var exception in pendingTasks.Tasks.Exceptions)
                    exceptions.Add(exception);
                foreach (var exception in invocationProvenance.Exceptions)
                {
                    if (!exceptions.Any(existing => ReferenceEquals(existing, exception)))
                        exceptions.Add(exception);
                }

                var failed = result == EventStatus.Failed || exceptions.Count > exceptionCount;
                session.Context.Context.Status = session.Executor.BehaviorStatus;
                if (session.Context.Context.Status == BehaviorStatus.Initialized)
                    session.Context.Context.Version = session.Executor.Graph.Version;
                if (!streamed && !failed && result != EventStatus.Undelivered)
                    session.Context.Context.LastExecutedAt = DateTime.Now;
                if (streamed || session.Context.IsStreamingBusyPeriod || (!failed && result != EventStatus.Undelivered))
                    await storage.DehydrateAsync(session.Context.Context);
                if (session.Executor.Initialized)
                    session.InitializationCompletion.TrySetResult(true);
                else if (!wasInitialized)
                    session.InitializationCompletion.TrySetResult(false);
                if (failed)
                    return EventStatus.Failed;
                return result == EventStatus.Undelivered && streamed ? EventStatus.Failed : result;
            }
            catch
            {
                if (!wasInitialized)
                    session.InitializationCompletion.TrySetResult(false);
                throw;
            }
        }
        private async Task<ActivitySession> GetOrCreateSessionAsync(BehaviorId id, bool streamed)
        {
            if (activitySessions.TryGetValue(id, out var existing))
                return existing;

            var gate = sessionCreationGates.GetOrAdd(id, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                if (activitySessions.TryGetValue(id, out existing))
                    return existing;

                var storedContext = await storage.HydrateAsync(id);
                var stateflowsContext = StateflowsJsonConverter.Clone(storedContext);
                stateflowsContext.Stored = storedContext.Stored;
                stateflowsContext.Deleted = storedContext.Deleted;
                var key = stateflowsContext.Version != 0
                    ? $"{id.Name}.{stateflowsContext.Version}"
                    : $"{id.Name}.current";
                if (!register.Activities.TryGetValue(key, out var graph))
                    return null;

                var serviceScope = new StateflowsServiceScope(provider.CreateAsyncScope());
                try
                {
                    using var pendingTasks = new PendingTasksScope();
                    var executor = new Executor(register, graph, serviceScope.ServiceProvider) { SessionOwned = true };
                    using var inspection = executor.BindInspection();
                    var rootContext = new RootContext(stateflowsContext) { IsStreamingExecution = streamed };
                    await executor.HydrateAsync(rootContext);
                    var created = new ActivitySession(serviceScope, executor, rootContext, streamed);
                    executor.StreamedWorkHost = work => HostStreamedSessionWork(id, created, work);
                    activitySessions[id] = created;
                    return created;
                }
                catch
                {
                    await serviceScope.DisposeAsync();
                    throw;
                }
            }
            finally
            {
                gate.Release();
            }
        }

        private async Task<EventStatus> ExecuteTypedSessionEventAsync<TEvent>(ActivitySession session, EventHolder<TEvent> eventHolder, bool regular)
        {
            var id = session.Context.Context.Id;
            if (regular && await TryCancelAsync(id, eventHolder, EventStatus.Undelivered, session.Executor))
                return EventStatus.Cancelled;

            var result = await ExecuteBehaviorAsync(eventHolder, EventStatus.Undelivered, session.Executor);

            if (regular && await TryCancelAsync(id, eventHolder, result, session.Executor))
                result = EventStatus.Cancelled;

            return result;
        }

        private Task<EventStatus> ExecuteTypedSessionEventAsync(ActivitySession session, EventHolder eventHolder, bool regular)
        {
            var method = typeof(Processor).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(candidate =>
                    candidate.Name == nameof(ExecuteTypedSessionEventAsync) &&
                    candidate.IsGenericMethodDefinition)
                ?.MakeGenericMethod(eventHolder.PayloadType);
            if (method == null)
                throw new InvalidOperationException($"No Activity event executor exists for {eventHolder.PayloadType}.");

            return (Task<EventStatus>)method.Invoke(this, [session, eventHolder, regular])!;
        }

        public async Task EndActivitySessionAsync(BehaviorId id, bool persist = true)
        {
            if (activitySessions.TryRemove(id, out var session))
            {
                using var inspection = session.Executor.BindInspection();
                session.Context.EndStreamingBusyPeriod();
                if (persist)
                    await storage.DehydrateAsync(session.Context.Context);
                await session.Executor.DisposeAsync();
                await session.ServiceScope.DisposeAsync();
            }
        }

        private sealed class ActivitySession
        {
            private int streaming;
            public StateflowsServiceScope ServiceScope { get; }
            public Executor Executor { get; }
            public RootContext Context { get; }
            public TaskCompletionSource<bool> InitializationCompletion { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            public ActivitySession(StateflowsServiceScope serviceScope, Executor executor, RootContext context, bool streamed)
            {
                ServiceScope = serviceScope;
                Executor = executor;
                Context = context;
                if (streamed)
                    BeginStreaming();
                if (executor.Initialized)
                    InitializationCompletion.TrySetResult(true);
            }

            public void BeginStreaming()
            {
                if (Interlocked.Exchange(ref streaming, 1) == 0)
                    Context.BeginStreamingBusyPeriod();
            }
        }

        private readonly ConcurrentDictionary<BehaviorId, Func<Func<Task>, bool>> sessionWorkHosts = new();

        public void SetActivitySessionWorkHost(BehaviorId id, Func<Func<Task>, bool> host)
        {
            if (host == null)
                sessionWorkHosts.TryRemove(id, out _);
            else
                sessionWorkHosts[id] = host;
        }

        /// <summary>
        /// Hands a firing that a regular transaction routed into streamed (mixed) work to the runtime, so the
        /// regular transaction closes without awaiting it. The work runs with streamed provenance and its own
        /// pending-task ownership; the live marking is dehydrated to the runtime after it completes.
        /// </summary>
        private bool HostStreamedSessionWork(BehaviorId id, ActivitySession session, Func<Task> work)
        {
            if (!sessionWorkHosts.TryGetValue(id, out var host) ||
                !activitySessions.TryGetValue(id, out var current) ||
                !ReferenceEquals(current, session))
                return false;

            return host(async () =>
            {
                using var inspection = session.Executor.BindInspection();
                using var pendingTasks = new PendingTasksScope();
                using var invocationProvenance = session.Context.BeginInvocationProvenance(streamed: true, callRoot: true);
                session.Context.IsStreamingExecution = true;
                var exceptions = new List<Exception>();
                try
                {
                    await work();
                }
                catch (ActivityCheckpointPauseException)
                {
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
                await pendingTasks.Tasks.DrainAsync();
                exceptions.AddRange(pendingTasks.Tasks.Exceptions);
                if (activitySessions.TryGetValue(id, out current) && ReferenceEquals(current, session))
                {
                    session.Context.Context.Status = session.Executor.BehaviorStatus;
                    await storage.DehydrateAsync(session.Context.Context);
                }
                if (exceptions.Count > 0)
                    throw new AggregateException(exceptions);
            });
        }

        public Task<TimeEvent[]> GetActivitySessionTimeEventsAsync(BehaviorId id)
            => Task.FromResult(activitySessions.TryGetValue(id, out var session)
                ? session.Context.Context.PendingTimeEvents.Values.ToArray()
                : []);

        public async Task<bool?> WaitForActivitySessionInitializationAsync(BehaviorId id)
        {
            if (!activitySessions.TryGetValue(id, out var session))
                return null;

            return await session.InitializationCompletion.Task;
        }

        public bool IsCurrentActivityInvocationStreamed(BehaviorId id)
            => activitySessions.TryGetValue(id, out var session) &&
                (session.Context.IsStreamingExecution || session.Context.IsCurrentInvocationStreamed);

        public async Task RequestActivitySessionCheckpointPauseAsync(BehaviorId id)
        {
            if (activitySessions.TryGetValue(id, out var session))
                await session.Executor.RequestCheckpointPauseAsync();
        }

        public async Task<bool> ResumeCheckpointedActivityAsync(BehaviorId id)
        {
            using var pendingTasks = new PendingTasksScope();
            var session = await GetOrCreateSessionAsync(id, streamed: false);
            if (session == null)
                return false;
            using var inspection = session.Executor.BindInspection();
            if (!session.Context.HasReadyFirings)
            {
                await EndActivitySessionAsync(id, persist: false);
                return false;
            }

            using var provenance = session.Context.BeginInvocationProvenance(streamed: true, callRoot: true);
            try
            {
                await session.Executor.ResumeReadyFiringsAsync();
                await pendingTasks.Tasks.DrainAsync();
                foreach (var exception in pendingTasks.Tasks.Exceptions)
                    session.Context.Context.ExecutionExceptions.Enqueue(exception);
                session.Context.Context.Status = session.Executor.BehaviorStatus;
                await storage.DehydrateAsync(session.Context.Context);
                return true;
            }
            catch
            {
                await EndActivitySessionAsync(id, persist: false);
                throw;
            }
        }

        private Task<EventStatus> TryHandleEventAsync<TEvent>(EventContext<TEvent> context)
        {
            var eventHandler = eventHandlers.FirstOrDefault(h =>
                h.EventType.IsGenericType && (context.Event?.GetType().IsGenericType ?? false)
                    ? context.Event.GetType().GetGenericTypeDefinition() == h.EventType
                    : h.EventType.IsInstanceOfType(context.Event)
            );

            return eventHandler != null
                ? eventHandler.TryHandleEventAsync(context)
                : Task.FromResult(EventStatus.NotConsumed);
        }

        private Task<EventStatus> ProcessRegularEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            var streamedExecution = eventHolder.Headers.Values.Any(header => header is StreamedActivityExecution);
            return ContextExecution.RunAsync(storage, id, exceptions, async execution =>
        {
            var result = EventStatus.Undelivered;

            await using var serviceScope = new StateflowsServiceScope(provider.CreateAsyncScope());
            var serviceProvider = serviceScope.ServiceProvider;

            var stateflowsContext = eventHolder.Headers.Values.Any(h => h is ForcedReset)
                ? new StateflowsContext(id)
                : execution.Context;
            execution.Context = stateflowsContext;

            stateflowsContext.ExecutionTriggerHolder = eventHolder;

            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!register.Activities.TryGetValue(key, out var graph))
            {
                return result;
            }

            var embedding = (BehaviorEmbedding?)eventHolder.Headers.Values.FirstOrDefault(h => h is BehaviorEmbedding);
            if (embedding != null)
            {
                stateflowsContext.ContextOwnerId = embedding.OwnerId;
                stateflowsContext.ContextParentId = embedding.ParentId;
            }

            await using var executor = new Executor(register, graph, serviceProvider);
            using var inspection = executor.BindInspection();

            var context = new RootContext(stateflowsContext);

            await executor.HydrateAsync(context);
            context.IsStreamingExecution = streamedExecution;
            if (streamedExecution)
            {
                context.RecolorMarkingAsStreamed();
            }
            try
            {
                result = await TryCancelAsync(id, eventHolder, result, executor)
                    ? EventStatus.Cancelled
                    : await ExecuteBehaviorAsync(eventHolder, result, executor);

                if (await TryCancelAsync(id, eventHolder, result, executor))
                {
                    result = EventStatus.Cancelled;
                }
            }
            finally
            {
                stateflowsContext.Status = executor.BehaviorStatus;

                if (stateflowsContext.Status == BehaviorStatus.Initialized)
                {
                    stateflowsContext.Version = graph.Version;
                }

                exceptions.AddRange(context.Exceptions);

                stateflowsContext = (await executor.DehydrateAsync()).Context;

                execution.Context = stateflowsContext;
            }

            return result;
        }, streamedExecution);
        }

        public Task CancelProcessingAsync(BehaviorId id)
        {
            lock (Common.Context.Classes.BaseContext.Instances)
            {
                if (Common.Context.Classes.BaseContext.Instances.TryGetValue(id, out var contextList))
                {
                    foreach (var context in contextList)
                    {
                        context.CancellationTokenSource.Cancel();
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task<bool> TryCancelAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, EventStatus result, Executor executor)
        {
            if (eventHolder.Payload is Finalize)
            {
                return false;
            }

            var forceFinalize = await valueStorage.GetOrDefaultAsync(id, CommonValues.ForceFinalizeKey, false);
            if (forceFinalize)
            {
                await valueStorage.RemoveAsync(id, CommonValues.ForceFinalizeKey);

                try
                {
                    var finalize = new Finalize().ToEventHolder();
                    executor.Context.SetEvent(finalize);
                    _ = await executor.CancelAsync();
                }
                finally
                {
                    executor.Context.ClearEvent();
                }

                return true;
            }

            return false;
        }

        Task<EventStatus> IStateflowsProcessor.ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder, EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => ExecuteBehaviorAsync(eventHolder, result, stateflowsExecutor as Executor);

        private async Task<EventStatus> ExecuteBehaviorAsync<TEvent>(
            EventHolder<TEvent> eventHolder,
            EventStatus result,
            Executor executor
        )
        {
            using var executionScope = executor.Context.BeginEventExecution();
            executor.Context.SetEvent(eventHolder);
            var streamedExecution = eventHolder.Headers.Values.Any(header => header is StreamedActivityExecution);
            try
            {
                var eventContext = new EventContext<TEvent>(executor.Context, executor.NodeScope);

                return await executor.Inspector.ProcessEventAsync(eventContext, async () =>
                {
                    var inputConsumedDuringInitialization = false;
                    var noImplicitInitialization =
                        eventHolder.PayloadType.GetCustomAttributes<NoImplicitInitializationAttribute>().Any() ||
                        eventHolder.Headers.Values.Any(h => h is NoImplicitInitialization);

                    if (eventHolder.Payload is TokensInputEvent inputEvent && !executor.Graph.AcceptsInput(inputEvent.Tokens))
                        return EventStatus.Rejected;

                    if (!executor.Initialized && !noImplicitInitialization)
                    {
                        Func<Task> onInitialized = null;
                        if (activitySessions.TryGetValue(executor.Context.Id, out var activitySession))
                        {
                            onInitialized = async () =>
                            {
                                activitySession.InitializationCompletion.TrySetResult(true);
                                if (streamedExecution)
                                    await storage.DehydrateAsync(executor.Context.Context);
                            };
                        }
                        else if (streamedExecution)
                        {
                            onInitialized = () => storage.DehydrateAsync(executor.Context.Context);
                        }
                        result = await executor.InitializeAsync(
                            eventHolder,
                            eventHolder.Payload is TokensInputEvent tokensEvent
                                ? tokensEvent.Tokens
                                : null,
                            onInitialized
                        );
                        inputConsumedDuringInitialization =
                            eventHolder.Payload is TokensInputEvent && result == EventStatus.Consumed;
                    }

                    if (result != EventStatus.Initialized && !inputConsumedDuringInitialization)
                    {
                        var handlingResult = await TryHandleEventAsync(eventContext);

                        if (executor.Initialized)
                        {
                            if (
                                handlingResult != EventStatus.Consumed &&
                                handlingResult != EventStatus.Rejected &&
                                handlingResult != EventStatus.NotInitialized
                            )
                            {
                                result = await executor.ProcessAsync(eventHolder);
                            }
                            else
                            {
                                result = handlingResult;
                            }
                        }
                        else
                        {
                            result = result == EventStatus.NotInitialized
                                ? EventStatus.NotInitialized
                                : noImplicitInitialization
                                    ? handlingResult
                                    : EventStatus.Rejected;
                        }
                    }

                    return result == EventStatus.Undelivered ? EventStatus.Failed : result;
                });
            }
            finally
            {
                executor.Context.ClearEvent();
            }
        }
    }
}
