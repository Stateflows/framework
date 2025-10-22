using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider ServiceProvider;

        public Processor(
            ActionsRegister register,
            IStateflowsLock stateflowsLock,
            IServiceProvider serviceProvider
        )
        {
            Register = register;
            StateflowsLock = stateflowsLock;
            ServiceProvider = serviceProvider;
        }

        public async Task<EventStatus> ProcessEventAsync<TEvent>(BehaviorId id, EventHolder<TEvent> eventHolder, List<Exception> exceptions)
        {
            var result = EventStatus.Undelivered;

            using var serviceScope = ServiceProvider.CreateScope();
            
            var serviceProvider = serviceScope.ServiceProvider;

            using var storage = serviceProvider.GetRequiredService<IStateflowsStorage>();

            var stateflowsContext = await storage.HydrateAsync(id);
            
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
                    stateflowsContext = await storage.HydrateAsync(id);
                }

                var executor = new Executor(Register, stateflowsContext, serviceProvider, action);
                
                await executor.HydrateAsync(eventHolder);
                
                if (eventHolder is EventHolder<CompoundRequest> compoundRequestHolder)
                {
                    var compoundRequest = compoundRequestHolder.Payload;
                    result = EventStatus.Consumed;
                    var results = new List<RequestResult>();
                    foreach (var ev in compoundRequest.Events)
                    {
                        ev.Headers.AddRange(compoundRequestHolder.Headers);

                        var status = await ev.ExecuteBehaviorAsync(this, result, executor);

                        results.Add(new RequestResult(
                            ev.GetResponseHolder(),
                            status,
                            new EventValidation(true, new List<ValidationResult>())
                        ));
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

                await storage.DehydrateAsync(stateflowsContext);
            }

            return result;
        }

        public Task<EventStatus> ExecuteBehaviorAsync<TEvent>(EventHolder<TEvent> eventHolder,
            EventStatus result, IStateflowsExecutor stateflowsExecutor)
            => stateflowsExecutor.DoProcessAsync(eventHolder);
    }
}
