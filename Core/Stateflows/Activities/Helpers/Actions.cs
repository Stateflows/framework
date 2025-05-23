using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public struct ActionNodeGlobalValueExpression
    {
        private readonly string ValueName;

        public ActionNodeGlobalValueExpression(string valueName)
        {
            ValueName = valueName;
        }

        /// <summary>
        /// Declares an action that sets the specified global value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <returns>State action function.</returns>
        public Func<IActivityActionContext, Task> Set<T>(T value)
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
        public Func<IActivityActionContext, Task> Update<T>(Func<T, T> valueUpdater, T defaultValue = default)
        {
            var self = this;
            return c => c.Behavior.Values.UpdateAsync(self.ValueName, valueUpdater, defaultValue);
        }

        /// <summary>
        /// Declares an action that removes the specified global value.
        /// </summary>
        /// <returns>State action function.</returns>
        public Func<IActivityActionContext, Task> Remove
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.RemoveAsync(self.ValueName);
            }
        }
    }

    public struct StateActionGlobalNamespaceExpression
    {
        private readonly string ValueSetName;

        public StateActionGlobalNamespaceExpression(string valueSetName)
        {
            ValueSetName = valueSetName;
        }

        /// <summary>
        /// Declares declarative actions based on the global value set's value with the given name.
        /// </summary>
        /// <param name="valueName">Name of the global value set's value used in the action.</param>
        /// <returns>Declarative actions.</returns>
        public ActionNodeGlobalValueExpression Value(string valueName)
        {
            var self = this;
            return new ActionNodeGlobalValueExpression($"{self.ValueSetName}.{valueName}");
        }

        /// <summary>
        /// Declares an action that clears all values in the specified global value set.
        /// </summary>
        /// <returns>State action function.</returns>
        public Func<IActivityActionContext, Task> Clear
        {
            get
            {
                var self = this;
                return c =>
                    ((ContextValuesCollection)c.Behavior.Values).RemoveMatchingAsync(
                        new Regex($"{self.ValueSetName}[.](.*)", RegexOptions.None, TimeSpan.FromSeconds(1)));
            }
        }
    }
    
    public struct ActionNodeGlobalSelector
    {
        /// <summary>
        /// Provides declarative guards based on the global value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public ActionNodeGlobalValueExpression Value(string valueName)
            => new ActionNodeGlobalValueExpression(valueName);
        
        /// <summary>
        /// Provides declarative guards based on the global value set with given name.
        /// </summary>
        /// <param name="valueSetName">Name of the global value set used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public StateActionGlobalNamespaceExpression Namespace(string valueSetName)
            => new StateActionGlobalNamespaceExpression(valueSetName);
    }
    
    public static class Actions
    {
        /// <summary>
        /// Declares an empty action that does nothing.
        /// </summary>
        public static Task Empty(IActivityActionContext _)
            => Task.CompletedTask;

        /// <summary>
        /// Provides declarative actions based on global values.
        /// </summary>
        public static ActionNodeGlobalSelector Global
            => new ActionNodeGlobalSelector();
    }
}
