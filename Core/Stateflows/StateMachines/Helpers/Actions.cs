using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Builds deferred actions that mutate one machine-global context value.</summary>
    public struct StateActionGlobalValueExpression
    {
        private readonly string ValueName;

        /// <summary>Selects a machine-global value without reading or changing it immediately.</summary>
        /// <param name="valueName">The context value key.</param>
        public StateActionGlobalValueExpression(string valueName)
        {
            ValueName = valueName;
        }

        /// <summary>
        /// Declares an action that sets the specified global value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <returns>State action function.</returns>
        public Func<IStateMachineActionContext, Task> Set<T>(T value)
        {
            var self = this;
            return c => c.Behavior.Values.SetAsync(self.ValueName, value);
        }

        /// <summary>
        /// Declares an action that updates the specified global value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="valueUpdater">Function to update the value.</param>
        /// <param name="defaultValue">Default value if the value is not set.</param>
        /// <returns>State action function.</returns>
        public Func<IStateMachineActionContext, Task> Update<T>(Func<T, T> valueUpdater, T defaultValue = default)
        {
            var self = this;
            return c => c.Behavior.Values.UpdateAsync(self.ValueName, valueUpdater, defaultValue);
        }

        /// <summary>
        /// Declares an action that removes the specified global value.
        /// </summary>
        /// <returns>State action function.</returns>
        public Func<IStateMachineActionContext, Task> Remove
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.RemoveAsync(self.ValueName);
            }
        }
    }

    /// <summary>Builds actions addressing machine-global values under a dot-separated prefix.</summary>
    public struct StateActionGlobalNamespaceExpression
    {
        private readonly string NamespaceName;

        /// <summary>Selects a machine-global value namespace.</summary>
        /// <param name="namespaceName">The prefix to which a dot and value name are appended.</param>
        public StateActionGlobalNamespaceExpression(string namespaceName)
        {
            NamespaceName = namespaceName;
        }

        /// <summary>
        /// Declares declarative actions based on the global value set's value with the given name.
        /// </summary>
        /// <param name="valueName">Name of the global value set's value used in the action.</param>
        /// <returns>Declarative actions.</returns>
        public StateActionGlobalValueExpression Value(string valueName)
        {
            var self = this;
            return new StateActionGlobalValueExpression($"{self.NamespaceName}.{valueName}");
        }

        /// <summary>
        /// Declares an action that clears all values in the specified global value set.
        /// </summary>
        /// <returns>State action function.</returns>
        public Func<IStateMachineActionContext, Task> Clear
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.RemovePrefixedAsync($"{self.NamespaceName}.");
            }
        }
    }
    
    /// <summary>Builds deferred actions that mutate one value scoped to the executing state.</summary>
    public struct StateActionStateValueExpression
    {
        private readonly string ValueName;

        /// <summary>Selects a state-local value without reading or changing it immediately.</summary>
        /// <param name="valueName">The state-local context value key.</param>
        public StateActionStateValueExpression(string valueName)
        {
            ValueName = valueName;
        }

        /// <summary>
        /// Declares an action that sets the specified state value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Set<T>(T value)
        {
            var self = this;
            return c => c.State.Values.SetAsync(self.ValueName, value);
        }

        /// <summary>
        /// Declares an action that updates the specified state value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="valueUpdater">Function to update the value.</param>
        /// <param name="defaultValue">Default value if the value is not set.</param>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Update<T>(Func<T, T> valueUpdater, T defaultValue = default)
        {
            var self = this;
            return c => c.State.Values.UpdateAsync(self.ValueName, valueUpdater, defaultValue);
        }

        /// <summary>
        /// Declares an action that removes the specified state value.
        /// </summary>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Remove
        {
            get
            {
                var self = this;
                return c => c.State.Values.RemoveAsync(self.ValueName);
            }
        }
    }

    /// <summary>Builds state-local value mutations using a dot-separated namespace prefix.</summary>
    /// <remarks>The Clear callback currently removes matching machine-global values, unlike Set, Update, and Remove.</remarks>
    public struct StateActionStateNamespaceExpression
    {
        private readonly string NamespaceName;

        /// <summary>Selects a state-value namespace for later callbacks.</summary>
        /// <param name="namespaceName">The prefix to which a dot and value name are appended.</param>
        public StateActionStateNamespaceExpression(string namespaceName)
        {
            NamespaceName = namespaceName;
        }

        /// <summary>
        /// Declares an action that sets the specified state value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="valueName">Name of value to set.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Set<T>(string valueName, T value)
        {
            var self = this;
            return c => c.State.Values.SetAsync($"{self.NamespaceName}.{valueName}", value);
        }

        /// <summary>
        /// Declares an action that updates the specified state value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="valueName">Name of value to set.</param>
        /// <param name="valueUpdater">Function to update the value.</param>
        /// <param name="defaultValue">Default value if the value is not set.</param>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Update<T>(string valueName, Func<T, T> valueUpdater, T defaultValue = default)
        {
            var self = this;
            return c => c.State.Values.UpdateAsync($"{self.NamespaceName}.{valueName}", valueUpdater, defaultValue);
        }

        /// <summary>
        /// Declares an action that removes the specified state value.
        /// </summary>
        /// <returns>State action function.</returns>
        /// <param name="valueName">The value name relative to the selected state-local namespace.</param>
        public Func<IStateActionContext, Task> Remove(string valueName)
        {
            var self = this;
            return c => c.State.Values.RemoveAsync($"{self.NamespaceName}.{valueName}");
        }

        /// <summary>
        /// Declares an action that removes machine-global values having this namespace prefix.
        /// </summary>
        /// <remarks>This callback uses the behavior value store rather than the executing state's value store.</remarks>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Clear
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.RemovePrefixedAsync($"{self.NamespaceName}.");
            }
        }
    }

    
    /// <summary>Selects machine-global values or namespaces for declarative state actions.</summary>
    public struct StateActionGlobalSelector
    {
        /// <summary>
        /// Selects a machine-global value for declarative mutation actions.
        /// </summary>
        /// <param name="valueName">The machine-global value key.</param>
        /// <returns>An expression that creates mutation callbacks for this value.</returns>
        public StateActionGlobalValueExpression Value(string valueName)
            => new StateActionGlobalValueExpression(valueName);
        
        /// <summary>
        /// Selects a machine-global value namespace for declarative mutation actions.
        /// </summary>
        /// <param name="namespaceName">The dot-separated value prefix.</param>
        /// <returns>An expression for values under the namespace.</returns>
        public StateActionGlobalNamespaceExpression Namespace(string namespaceName)
            => new StateActionGlobalNamespaceExpression(namespaceName);
    }

    /// <summary>Selects state-local values or namespaces for declarative state actions.</summary>
    public struct StateActionStateSelector
    {
        /// <summary>
        /// Selects a state-local value for declarative mutation actions.
        /// </summary>
        /// <param name="valueName">The executing state's value key.</param>
        /// <returns>An expression that creates mutation callbacks for this value.</returns>
        public StateActionStateValueExpression Value(string valueName)
        {
            var self = this;
            return new StateActionStateValueExpression(valueName);
        }
        
        /// <summary>
        /// Selects a namespace prefix for declarative state-value mutation actions.
        /// </summary>
        /// <param name="namespaceName">The dot-separated value prefix.</param>
        /// <returns>An expression for values under the namespace.</returns>
        public StateActionStateNamespaceExpression Namespace(string namespaceName)
        {
            var self = this;
            return new StateActionStateNamespaceExpression(namespaceName);
        }
    }
    
    /// <summary>Provides reusable state-action callbacks and deferred value-mutation expressions.</summary>
    /// <remarks>Constructing an expression does not mutate values; changes occur when its returned callback is executed.</remarks>
    public static class Actions
    {
        /// <summary>
        /// Declares an empty action that does nothing.
        /// </summary>
        /// <param name="_">The execution context, intentionally unused.</param>
        /// <returns>An already completed task.</returns>
        public static Task Empty(IStateMachineActionContext _)
            => Task.CompletedTask;

        /// <summary>
        /// Provides declarative actions based on global values.
        /// </summary>
        public static StateActionGlobalSelector Global
            => new StateActionGlobalSelector();
        
        /// <summary>
        /// Provides declarative actions based on the state values.
        /// </summary>
        public static StateActionStateSelector State
            => new StateActionStateSelector();
    }
}
