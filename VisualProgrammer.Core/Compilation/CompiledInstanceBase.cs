﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core.Compilation {

	/// <summary>
	/// A base class that the program instances generated by compilation will inherit.
	/// </summary>
	public abstract class CompiledInstanceBase : DynamicObject, IAnonymousProgram, ICompiledInstanceBase {

		// The delegates for each of the compiled functions
		private readonly Dictionary<string, Delegate> functions;
		private readonly Dictionary<string, (Type type, object @default)> variableDefinitions;
		private Dictionary<string, object> variableValues;

#pragma warning disable CS8618 // Disable warning saying that variableValues is unset, because the compiler doesn't realise it's set during the ResetVariables() call.
		protected CompiledInstanceBase(Dictionary<string, Delegate> functions, Dictionary<string, (Type type, object @default)> variableDefinitions) {
			this.functions = new Dictionary<string, Delegate>(functions ?? new Dictionary<string, Delegate>(), StringComparer.OrdinalIgnoreCase);
			this.variableDefinitions = new Dictionary<string, (Type type, object @default)>(variableDefinitions ?? new Dictionary<string, (Type type, object @default)>(), StringComparer.OrdinalIgnoreCase);
			ResetVariables();
		}
#pragma warning restore CS8618

		/// <summary>
		/// Resets all variables to their default values as defined by the given variable definitions dictionary.
		/// </summary>
		public void ResetVariables() => variableValues = new Dictionary<string, object>(variableDefinitions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.@default), StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Attempts to get the value of the variable with the given name.
		/// </summary>
		/// <param name="key">The name/key of the variable. Case-insensitive.</param>
		/// <exception cref="ArgumentException">When a variable with the target name has not been defined.</exception>
		public object GetVariable(string key) {
			if (!variableDefinitions.ContainsKey(key))
				throw new ArgumentException($"Variable '{key}' has not been defined.", nameof(key));
			return variableValues[key];
		}

		/// <summary>
		/// Attempts to set the value of the variable with the given name to the given value.
		/// </summary>
		/// <param name="key">The name/key of the variable. Case-insensitive.</param>
		/// <exception cref="ArgumentException">When a variable with the target name has not been defined or the given value cannot be assigned to the type defined by the target variable.</exception>
		public void SetVariable(string key, object value) {
			if (!variableDefinitions.TryGetValue(key, out var def))
				throw new ArgumentException($"Variable '{key}' has not been defined.", nameof(key));
			if (!def.type.CanBeSetTo(value))
				throw new ArgumentException($"Value of '{value?.ToString() ?? "null"}' cannot be assigned to variable of type '{def.type.Name}'.", nameof(value));
			variableValues[key] = value;
		}

		/// <summary>Invokes the target delegate method with the given arguments.</summary>
		/// <param name="name">The name of the function to execute</param>
		/// <param name="args">The arguments passed to the compiled delegate.</param>
		/// <returns><c>true</c> if the function was invoked, <c>false</c> if no delegate was found.</returns>
		protected internal bool ExecuteFunction(string name, object[] args) {
			if (!functions.TryGetValue(name, out var @delegate))
				return false;
			// TODO: Optimise this. Ideally without needing to allocate arrays, rely on a dynamic invocation
			@delegate.DynamicInvoke(new object[] { this }.Concat(args).ToArray());
			return true;
		}

		/// <inheritdoc />
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object? result) {
			result = null; // The functions can't (currently, atleast) return anything
			return ExecuteFunction(binder.Name, args);
		}
	}


	/// <summary>
	/// An interface that is implemented by the <see cref="CompiledInstanceBase"/>.
	/// Custom program interfaces can extend this interface to provide strict-typing for getting or setting non-property-bound variables.
	/// </summary>
	public interface ICompiledInstanceBase {
		/// <summary>
		/// Resets all variables to their default values as defined by the given variable definitions dictionary.
		/// </summary>
		void ResetVariables();

		/// <summary>
		/// Attempts to get the value of the variable with the given name.
		/// </summary>
		/// <param name="key">The name/key of the variable. Case-insensitive.</param>
		object GetVariable(string key);

		/// <summary>
		/// Attempts to set the value of the variable with the given name to the given value.
		/// </summary>
		/// <param name="key">The name/key of the variable. Case-insensitive.</param>
		void SetVariable(string key, object value);
	}
}
