using System;
using System.Collections.Generic;

namespace Stateflows.Common.Models
{
    internal class Logic<TDelegate>
        where TDelegate : Delegate
    {
        public IList<TDelegate> Actions { get; set; } = new List<TDelegate>();

        public string Name { get; set; }
    }
}