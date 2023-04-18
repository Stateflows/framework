namespace Stateflows.StateMachines.Context.Interfaces
{
    public interface IContextValues
    {
        void Set<T>(string key, T value);

        bool IsSet(string key);

        bool TryGet<T>(string key, out T value);

        T GetOrDefault<T>(string key, T defaultValue);

        void Remove(string key);

        void Clear();
    }
}
