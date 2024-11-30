using System;
using System.Linq;
using System.Collections.Generic;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;
using Stateflows.Common.Utilities;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class StateMachineInspection : IStateMachineInspection
    {
        private Executor Executor { get; }

        public StateMachineInspection(Executor executor)
        {
            Executor = executor;

            Executor.Inspector.InitializeInspection = new ActionInspection(Executor, nameof(Initialize));
            Initialize = Executor.Inspector.InitializeInspection;
            Executor.Inspector.FinalizeInspection = new ActionInspection(Executor, nameof(Finalize));
            Finalize = Executor.Inspector.FinalizeInspection;
        }

        public StateMachineId Id => Executor.Context.Id;

        private IEnumerable<IStateInspection> states;

        public IEnumerable<IStateInspection> States
            => states ??= Executor.Graph.Vertices.Values.Select(v => new StateInspection(Executor, v)).ToArray();

        public IEnumerable<IStateInspection> CurrentStatesStack
        {
            get
            {
                throw new NotImplementedException("implement states tree inspection instead of states stack inspection");
                //var result = new List<IStateInspection>();
                //IEnumerable<IStateInspection> statesSet = States;
                //foreach (var vertex in Executor.VerticesStack)
                //{
                //    var state = statesSet.First(s => s.Name == vertex.Name);
                //    if (state != null)
                //    {
                //        result.Add(state);
                //        statesSet = state.States;
                //    }
                //}
                //return result;
            }
        }

        public IReadOnlyTree<IStateInspection> CurrentStatesTree
        {
            get
            {
                throw new NotImplementedException("implement states tree inspection instead of states stack inspection");
            }
        }

        public IActionInspection Initialize { get; set; }

        public IActionInspection Finalize { get; set; }

        public bool StateHasChanged => Executor.StateHasChanged;
    }
}
