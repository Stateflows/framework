using System;
using System.Collections.Generic;

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

        public string Name { get; }
    }
}