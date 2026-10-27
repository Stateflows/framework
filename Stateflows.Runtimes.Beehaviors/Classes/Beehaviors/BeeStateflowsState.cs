using System.Text.Json.Serialization;
using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;

namespace Stateflows;

/// <summary>Stores a runtime bee's serialized Stateflows context and its nonserialized in-memory representation.</summary>
public sealed class BeeStateflowsState
{
    /// <summary>Gets or sets the persisted serialized behavior context, or null when no context is stored.</summary>
    public string? SerializedContext { get; set; }

    /// <summary>Gets or sets the hydrated context excluded from both JSON serializers.</summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public StateflowsContext? Context { get; set; }

    /// <summary>Returns the cached context, restores the serialized context, or creates a new context.</summary>
    /// <param name="id">Behavior identity used when a new context must be created.</param>
    /// <returns>The hydrated or newly created context.</returns>
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

    /// <summary>Updates the serialized representation from the in-memory context.</summary>
    /// <remarks>A deleted context clears the stored representation. A null in-memory context leaves it unchanged.</remarks>
    public void SyncSerialized()
    {
        if (Context != null)
        {
            SerializedContext = Context.Deleted ? null : StateflowsJsonConverter.SerializePolymorphicObject(Context);
        }
    }
}
