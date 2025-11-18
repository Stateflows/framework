using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Context.Classes;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Actions.Registration;

namespace Stateflows.Actions.Engine
{
    internal class Processor : IEventProcessor, IStateflowsProcessor
    {
        string IEventProcessor.BehaviorType => BehaviorType.Action;

        private readonly ActionsRegister Register;
        private readonly IStateflowsLock StateflowsLock;
        private readonly IStateflowsStorage Storage;
        private readonly IServiceProvider ServiceProvider;

        public Processor(
            ActionsRegister register,
            IStateflowsLock stateflowsLock,
            IStateflowsStorage storage,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            StateflowsLock = stateflowsLock;
            Storage = storage;
            ServiceProvider = serviceProvider;
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            var result = EventStatus.Undelivered;

            using var serviceScope = ServiceProvider.CreateScope();
            
            var serviceProvider = serviceScope.ServiceProvider;

            var stateflowsContext = await Storage.HydrateAsync(id);
            
            var key = stateflowsContext.Version != 0
                ? $"{id.Name}.{stateflowsContext.Version}"
                : $"{id.Name}.current";

            if (!Register.Actions.TryGetValue(key, out var action))
            {
                return result;
            }

            try
            {
                await using var lockHandle = await (
                    action.Reentrant
                        ? StateflowsLock.AquireNoLockAsync(id)
                        : StateflowsLock.AquireLockAsync(id)
                );

                if (action.Reentrant)
                {
                    stateflowsContext = await Storage.HydrateAsync(id);
                }

                var executor = new Executor(Register, stateflowsContext, serviceProvider, action);
                
                await executor.HydrateAsync(eventHolder);
                
                if (eventHolder is EventHolder<CompoundRequest> compoundRequestHolder)
                {
                    var compoundRequest = compoundRequestHolder.Payload;
                    var compoundResponse = compoundRequest.GetResponse();
                    result = EventStatus.Consumed;
                    var results = new List<RequestResult>();
                    var i = -1;
                    foreach (var ev in compoundRequest.Events)
                    {
                        i++;
                                
                        RequestResult responseResult = null;
                        if (compoundResponse != null)
                        {
                            responseResult = ((List<RequestResult>)compoundResponse.Results)[i];
                            if (
                                responseResult?.Status == EventStatus.Invalid ||
                                (
                                    responseResult?.Status == EventStatus.Omitted &&
                                    !ev.Headers.Any(h => h is ForcedExecution)
                                )
                            )
                            {
                                continue;
                            }
                        }

                        ev.Headers.AddRange(compoundRequestHolder.Headers);

                        var status = await ev.ExecuteBehaviorAsync(this, result, executor);

                        if (responseResult != null)
                        {
                            responseResult.Status = status;
                            responseResult.Response = ev.IsRequest()
                                ? ev.GetResponseHolder()
                                : null;
                            responseResult.Validation = new EventValidation(true, new List<ValidationResult>());
                        }
                        else
                        {
                            results.Add(new RequestResult(
                                ev.GetResponseHolder(),
                                status,
                                new EventValidation(true, new List<ValidationResult>())
                            ));
                        }
                    }

                    if (!compoundRequest.IsRespondedTo())
                    {
                        compoundRequest.Respond(new CompoundResponse()
                        {
                            Results = results
                        });
                    }
                }
                else
                {
                    result = await ExecuteBehaviorAsync(eventHolder, result, executor);
                }
                
                await executor.DehydrateAsync(eventHolder);
            }
            finally
            {
                if (stateflowsContext.Status == BehaviorStatus.Initialized)
                {
                    stateflowsContext.Version = action.Version;
                }

                stateflowsContext.Status = BehaviorStatus.Initialized;

                stateflowsContext.LastExecutedAt = DateTime.Now;

                // exceptions.AddRange(context.Exceptions);

                await Storage.DehydrateAsync(stateflowsContext);
            }

            return result;
        }

        public Task CancelProcessingAsync(BehaviorId id)
        {
            lock (ActionDelegateContext.Instances)
            {
                if (ActionDelegateContext.Instances.TryGetValue(id, out var contextList))
                {
                    foreach (var context in contextList)
                    {
                        context.CancellationTokenSource.Cancel();
                    }
                }
            }
            
            return Task.CompletedTask;
        }

        public Task<EventStatus> ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder,
            EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => stateflowsExecutor.DoProcessAsync(eventHolder);
    }
}
