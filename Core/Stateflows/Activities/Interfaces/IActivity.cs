﻿using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;

namespace Stateflows.Activities.Interfaces
{
    public interface IActivity : IBehavior
    {
        Task<IEnumerable<TokenHolder>> Execute(IEnumerable<TokenHolder> input);

        Task<T> Execute<T>(IDictionary<string, object> parameters);

        Task Execute(IDictionary<string, object> parameters);
    }
}
