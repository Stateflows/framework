﻿using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities;
using Stateflows.StateMachines;
using Stateflows.Extensions.PlantUml.Events;

namespace Stateflows.Extensions.PlantUml.Classes
{
    internal class PlantUmlHandler : IStateMachineEventHandler, IActivityEventHandler
    {
        public Type EventType => typeof(PlantUmlRequest);

        public Task<EventStatus> TryHandleEventAsync<TEvent>(StateMachines.Inspection.Interfaces.IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
            => Task.FromResult(HandleEvent(context.Event, () => context.StateMachine.Inspection.GetPlantUml()));

        public Task<EventStatus> TryHandleEventAsync<TEvent>(Activities.Inspection.Interfaces.IEventInspectionContext<TEvent> context)
            where TEvent : Event, new()
            => Task.FromResult(HandleEvent(context.Event, () => context.Activity.Inspection.GetPlantUml()));

        private EventStatus HandleEvent<TEvent>(TEvent @event, Func<string> plantUmlGenerator)
            where TEvent : Event, new()
        {
            if (@event is PlantUmlRequest request)
            {
                var plantUml = plantUmlGenerator();
                request.Respond(new PlantUmlResponse() { PlantUml = plantUml });

                return EventStatus.Consumed;
            }

            return EventStatus.NotConsumed;
        }
    }
}
