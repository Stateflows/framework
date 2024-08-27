using System;

namespace Stateflows.Common.Interfaces
{
    public interface IContextValues
    {
        void Set<T>(string key, T value);

        bool IsSet(string key);

        bool TryGet<T>(string key, out T value);

        T GetOrDefault<T>(string key, T defaultValue = default);

        void Update<T>(string key, Func<T, T> valueUpdater, T defaultValue = default);

        void Remove(string key);

        void Clear();
    }
}
