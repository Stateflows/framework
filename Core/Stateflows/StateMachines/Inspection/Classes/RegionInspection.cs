﻿using System.Linq;
using System.Collections.Generic;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Engine;
using Stateflows.StateMachines.Inspection.Interfaces;

namespace Stateflows.StateMachines.Inspection.Classes
{
    internal class RegionInspection : IRegionInspection
    {
        private Executor Executor { get; }

        private Region Region { get; }

        public RegionInspection(Executor executor, Region region)
        {
            Executor = executor;
            Region = region;
        }

        public IEnumerable<IStateInspection> states;

        public IEnumerable<IStateInspection> States
        {
            get
            {
                states ??= Region.Vertices.Values.Select(subVertex => new StateInspection(Executor, subVertex)).ToArray();

                return states;
            }
        }
    }
}
