using System;

namespace Stateflows.Activities
{
    /// <summary>Configures implicit structural behavior for an activity node.</summary>
    [Flags]
    public enum NodeOptions
    {
        /// <summary>No implicit structural behavior is enabled.</summary>
        None = 0,
        /// <summary>Allows the graph builder to insert an implicit join where required.</summary>
        ImplicitJoin = 1,
        /// <summary>Allows the graph builder to insert an implicit fork where required.</summary>
        ImplicitFork = 2,
        //Reentrant = 4,
        /// <summary>Default options for ordinary action nodes.</summary>
        ActionDefault = ImplicitJoin | ImplicitFork,
        /// <summary>Default options for control nodes.</summary>
        ControlNodeDefault = None,
        /// <summary>Default options for decision nodes.</summary>
        DecisionDefault = ImplicitJoin,
        /// <summary>Default options for data-store nodes.</summary>
        DataStoreDefault = ImplicitFork
    }
}   
