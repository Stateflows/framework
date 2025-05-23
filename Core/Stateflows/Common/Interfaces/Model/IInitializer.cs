﻿using System.Threading.Tasks;

namespace Stateflows.Common
{
    public interface IInitializer<TInitializationEvent>
    {
        Task<bool> OnInitializeAsync(TInitializationEvent initializationEvent);
    }

    public interface IDefaultInitializer
    {
        Task<bool> OnInitializeAsync();
    }
}
