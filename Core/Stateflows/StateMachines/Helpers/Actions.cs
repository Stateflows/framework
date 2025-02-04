﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Stateflows.Common.Classes;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public struct StateActionGlobalValueExpression
    {
        private readonly string ValueName;

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
            return c => c.StateMachine.Values.SetAsync(self.ValueName, value);
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
            return c => c.StateMachine.Values.UpdateAsync(self.ValueName, valueUpdater, defaultValue);
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
                return c => c.StateMachine.Values.RemoveAsync(self.ValueName);
            }
        }
    }

    public struct StateActionGlobalValueSetExpression
    {
        private readonly string ValueSetName;

        public StateActionGlobalValueSetExpression(string valueSetName)
        {
            ValueSetName = valueSetName;
        }

        /// <summary>
        /// Declares declarative actions based on the global value set's value with the given name.
        /// </summary>
        /// <param name="valueName">Name of the global value set's value used in the action.</param>
        /// <returns>Declarative actions.</returns>
        public StateActionGlobalValueExpression Value(string valueName)
        {
            var self = this;
            return new StateActionGlobalValueExpression($"{self.ValueSetName}.{valueName}");
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
                return c =>
                    ((ContextValuesCollection)c.StateMachine.Values).RemoveMatchingAsync(
                        new Regex($"{self.ValueSetName}[.](.*)"));
            }
        }
    }
    
    public struct StateActionStateValueExpression
    {
        private readonly string ValueName;

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
            return c => c.CurrentState.Values.SetAsync(self.ValueName, value);
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
            return c => c.CurrentState.Values.UpdateAsync(self.ValueName, valueUpdater, defaultValue);
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
                return c => c.CurrentState.Values.RemoveAsync(self.ValueName);
            }
        }
    }

    public struct StateActionStateValueSetExpression
    {
        private readonly string ValueSetName;

        public StateActionStateValueSetExpression(string valueSetName)
        {
            ValueSetName = valueSetName;
        }

        /// <summary>
        /// Declares an action that sets the specified state value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Set<T>(string valueName, T value)
        {
            var self = this;
            return c => c.CurrentState.Values.SetAsync($"{self.ValueSetName}.{valueName}", value);
        }

        /// <summary>
        /// Declares an action that updates the specified state value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="valueUpdater">Function to update the value.</param>
        /// <param name="defaultValue">Default value if the value is not set.</param>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Update<T>(string valueName, Func<T, T> valueUpdater, T defaultValue = default)
        {
            var self = this;
            return c => c.CurrentState.Values.UpdateAsync($"{self.ValueSetName}.{valueName}", valueUpdater, defaultValue);
        }

        /// <summary>
        /// Declares an action that removes the specified state value.
        /// </summary>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Remove(string valueName)
        {
            var self = this;
            return c => c.CurrentState.Values.RemoveAsync($"{self.ValueSetName}.{valueName}");
        }

        /// <summary>
        /// Declares an action that clears all values in the specified state value set.
        /// </summary>
        /// <returns>State action function.</returns>
        public Func<IStateActionContext, Task> Clear
        {
            get
            {
                var self = this;
                return c => ((ContextValuesCollection)c.CurrentState.Values).RemoveMatchingAsync(
                    new Regex($"{self.ValueSetName}[.](.*)")
                );
            }
        }
    }

    
    public struct StateActionGlobalSelector
    {
        /// <summary>
        /// Provides declarative guards based on the global value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public StateActionGlobalValueExpression Value(string valueName)
            => new StateActionGlobalValueExpression(valueName);
        
        /// <summary>
        /// Provides declarative guards based on the global value set with given name.
        /// </summary>
        /// <param name="valueSetName">Name of the global value set used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public StateActionGlobalValueSetExpression ValueSet(string valueSetName)
            => new StateActionGlobalValueSetExpression(valueSetName);
    }

    public struct StateActionStateSelector
    {
        /// <summary>
        /// Provides declarative guards based on the state's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the state's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public StateActionStateValueExpression Value(string valueName)
        {
            var self = this;
            return new StateActionStateValueExpression(valueName);
        }
        
        /// <summary>
        /// Provides declarative guards based on the state's value set with given name.
        /// </summary>
        /// <param name="valueSetName">Name of the state's value set used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public StateActionStateValueSetExpression ValueSet(string valueSetName)
        {
            var self = this;
            return new StateActionStateValueSetExpression(valueSetName);
        }
    }
    
    public static class Actions
    {
        /// <summary>
        /// Declares an empty action that does nothing.
        /// </summary>
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
