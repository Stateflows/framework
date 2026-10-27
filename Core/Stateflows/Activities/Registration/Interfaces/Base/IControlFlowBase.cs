using System.Diagnostics;
using Stateflows.Activities.Extensions;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Adds control flows from the current node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    public interface IControlFlowBase<out TReturn>
    {
        /// <summary>Adds a control flow from the current node to the named target.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        TReturn AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
        
        /// <summary>Adds a control flow from the current node to the named target.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddControlFlow<TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        /// <summary>Adds a control flow from the current node to the named target.</summary>
        /// <typeparam name="TControlFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddControlFlow<TControlFlow>(string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddControlFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );

        /// <summary>Adds a control flow from the current node to the named target.</summary>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddControlFlow<TFlow, TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }
    
    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenControlFlowBase<out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction);
        
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseControlFlow<TTargetNode>(ControlFlowBuildAction buildAction)
            where TTargetNode : class, IActivityNode
            => UseControlFlow(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds fallback control flows from the current node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    public interface IElseControlFlowBase<out TReturn>
    {
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        TReturn AddElseControlFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
        
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddElseControlFlow<TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddElseControlFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TControlFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddElseControlFlow<TControlFlow>(string targetNodeName, ElseControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddElseControlFlow(
                targetNodeName,
                b => buildAction?.Invoke(b)
            );

        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddElseControlFlow<TFlow, TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddElseControlFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds decision control flows from the current decision node.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    public interface IDecisionFlowBase<out TReturn>
    {
        /// <summary>Adds a guarded control flow from the decision node to a target.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the control flow's guards and options. Its builder is scoped to
        /// the node or flow, not the enclosing Activity.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        TReturn AddFlow(string targetNodeName, ControlFlowBuildAction buildAction);
        
        /// <summary>Adds a guarded control flow from the decision node to a target.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddFlow<TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        /// <summary>Adds a guarded control flow from the decision node to a target.</summary>
        /// <typeparam name="TControlFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddFlow<TControlFlow>(string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );

        /// <summary>Adds a guarded control flow from the decision node to a target.</summary>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddFlow<TFlow, TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenDecisionFlowBase<out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Callback configuring the selected inherited element's replacement behavior. Its
        /// builder is scoped to that element.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseFlow(string targetNodeName, ControlFlowBuildAction buildAction);
        
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseFlow<TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseFlow(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds fallback decision control flows.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    public interface IElseDecisionFlowBase<out TReturn>
    {
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        TReturn AddElseFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
        
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddElseFlow<TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddElseFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TControlFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddElseFlow<TControlFlow>(string targetNodeName, ElseControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddElseFlow(
                targetNodeName,
                b => buildAction?.Invoke(b)
            );

        /// <summary>Adds a fallback flow used when no ordinary outgoing flow is eligible.</summary>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public TReturn AddElseFlow<TFlow, TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddElseFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenElseDecisionFlowBase<out TReturn>
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        TReturn UseElseFlow(string targetNodeName, ElseControlFlowBuildAction buildAction = null);
        
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public TReturn UseElseFlow<TTargetNode>(ElseControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseElseFlow(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Adds control flows where the builder does not return a fluent result.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    public interface IControlFlowBase
    {
        /// <summary>Adds a control flow from the current node to the named target.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        void AddControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
        
        /// <summary>Adds a control flow from the current node to the named target.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public void AddControlFlow<TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => AddControlFlow(ActivityNode<TTargetNode>.Name, buildAction);
        
        /// <summary>Adds a control flow from the current node to the named target.</summary>
        /// <typeparam name="TControlFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public void AddControlFlow<TControlFlow>(string targetNodeName, ControlFlowBuildAction buildAction = null)
            where TControlFlow : class, IControlFlow
            => AddControlFlow(
                targetNodeName,
                b =>
                {
                    b.AddControlFlowEvents<TControlFlow>();
                    buildAction?.Invoke(b);
                }
            );

        /// <summary>Adds a control flow from the current node to the named target.</summary>
        /// <typeparam name="TFlow">Typed flow implementation implementing <see cref="IControlFlow" />.</typeparam>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        [DebuggerHidden]
        public void AddControlFlow<TFlow, TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TFlow : class, IControlFlow
            where TTargetNode : class, IActivityNode
            => AddControlFlow<TFlow>(ActivityNode<TTargetNode>.Name, buildAction);
    }

    /// <summary>Provides fluent APIs for extending elements inherited from a reusable Activity definition.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
    /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
    public interface IOverridenControlFlowBase
    {
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <param name="targetNodeName">Registered name of the destination node in the containing Activity scope.</param>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        void UseControlFlow(string targetNodeName, ControlFlowBuildAction buildAction = null);
        
        /// <summary>Selects and replaces the matching inherited flow from the current node.</summary>
        /// <typeparam name="TTargetNode">Destination node implementation type; its registered node name selects the flow
        /// target.</typeparam>
        /// <param name="buildAction">Optional callback configuring the element selected by this registration method.
        /// </param>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/activity-control-flow" />
        /// <seealso href="https://github.com/Stateflows/framework/wiki/behaviors#definition-reuse-and-versions" />
        [DebuggerHidden]
        public void UseControlFlow<TTargetNode>(ControlFlowBuildAction buildAction = null)
            where TTargetNode : class, IActivityNode
            => UseControlFlow(ActivityNode<TTargetNode>.Name, buildAction);
    }
}
