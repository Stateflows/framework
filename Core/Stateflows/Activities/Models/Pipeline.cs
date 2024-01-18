using System;
using System.Collections.Generic;
using Stateflows.Common.Models;

namespace Stateflows.Activities.Models
{
    internal class Pipeline<TDelegate>
        where TDelegate : Delegate
    {
        public Graph Graph { get; set; }

        public IList<Logic<TDelegate>> Actions { get; set; } = new List<Logic<TDelegate>>();
    }
}