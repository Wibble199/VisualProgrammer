using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using VisualProgrammer.Core.Compilation;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Class that holds and manages variables for a visual program.
	/// </summary>
	public class VariableCollection : IEnumerable<Variable> {

		internal VisualProgram context;

		private Dictionary<string, Variable> definitions = new Dictionary<string, Variable>();

		/// <summary>
		/// A parameter that will be the first parameter of all compiled functions which provides them access to their instance context (e.g. allows for accessing variable store).
		/// </summary>
		internal readonly ParameterExpression compiledInstanceParameter = Expression.Parameter(typeof(CompiledInstanceBase), "context");

		/// <summary>
		/// Attempts to define a new variable on this program.
		/// </summary>
		/// <exception cref="ArgumentException">If the new variable's name is invalid or if the <paramref name="default"/> value cannot be assigned to type <paramref name="type"/>.</exception>
		/// <exception cref="ArgumentNullException">If the new variable's type is <c>null</c>.</exception>
		public void Add(string name, Type type, object? @default) {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name must not be null, empty or whitespace.", nameof(name));
			if (definitions.ContainsKey(name))
				throw new ArgumentException($"A variable with the name '{name}' already exists.", nameof(name));
			if (type == null)
				throw new ArgumentNullException(nameof(type), "Variable type must not be null.");
			definitions.Add(name, new Variable(name, type, @default));
		}

		/// <summary>
		/// Attempts to get the variable with the given name. If not found, returns null.
		/// </summary>
		public Variable? this[string name] => definitions.TryGetValue(name, out var @var) ? var : null;

		/// <summary>
		/// Attempts to get the variable with the given name. Returns true if found.
		/// </summary>
		public bool TryGetVariable(string name, [MaybeNullWhen(false), NotNullWhen(true)] out Variable? variable) => definitions.TryGetValue(name, out variable);

		/// <summary>
		/// Removes the variable with the given name from the program.<para/>
		/// Will remove the references from any VisualEntries's parameter mappings and any VisualNodes's VariableReference properties
		/// </summary>
		/// <param name="name"></param>
		public void Remove(string name) {
			if (definitions.Remove(name)) {
				// References to reset (cannot remove in the first loop as doing so modifies the collection and invalidates the enumerator)
				var paramMapsToReset = new List<(VisualEntry, string)>();
				var varRefsToReset = new List<(VisualNode, VisualNodePropertyDefinition)>();

				// If removal was successful, clean up some references to the variable
				foreach (var node in context!.Nodes) {
					// Find all entries and unmap any parameter mappings that use the removed variable
					if (node is VisualEntry entry) {
						foreach (var kvp in entry!.ParameterMap)
							if (kvp.Value == name)
								paramMapsToReset.Add((entry!, kvp.Key));
					} else {
						// Find all (non-entry) nodes for any variablereference properties that reference the removed variable
						foreach (var prop in node.GetPropertiesOfType(VisualNodePropertyType.Variable))
							if (prop.Getter(node) is IVariableReference varDef && varDef.Name == name)
								varRefsToReset.Add((node, prop));
					}
				}

				// Now that we have found parameters and variable references that need to be reset, we can do so
				foreach (var (entry, paramKey) in paramMapsToReset)
					entry.ParameterMap[paramKey] = "";
				foreach (var (node, prop) in varRefsToReset)
					prop.Setter(node, VariableReference.Create(prop.PropertyDataType!, "")); // Create a new empty reference of the relevant type
			}
		}

		#region IEnumerable
		public IEnumerator<Variable> GetEnumerator() => definitions.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		#endregion
	}
}
