using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateflows.Common.Models
{
    internal class Logic<TDelegate>
        where TDelegate : Delegate
    {
        public Logic(string name)
        {
             Name = name;
        }

        public List<TDelegate> Actions { get; set; } = new List<TDelegate>();

        public bool Any => Actions.Any();

        public string Name { get; }
    }
}