using System.Text.Json.Serialization;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;

namespace Stateflows;

public sealed class BeeStateflowsState
{
    public string? SerializedContext { get; set; }

    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public StateflowsContext? Context { get; set; }

    public StateflowsContext GetOrCreateContext(BehaviorId id)
    {
        if (Context != null)
        {
            return Context;
        }

        if (!string.IsNullOrEmpty(SerializedContext))
        {
            Context = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(SerializedContext);
            if (Context != null)
            {
                return Context;
            }
        }

        Context = new StateflowsContext(id);
        return Context;
    }

    public void SyncSerialized()
    {
        if (Context != null)
        {
            SerializedContext = Context.Deleted ? null : StateflowsJsonConverter.SerializePolymorphicObject(Context);
        }
    }
}
