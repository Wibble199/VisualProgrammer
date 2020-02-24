using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using VisualProgrammer.Core.Compilation;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Class that holds and manages variables for a visual program.
	/// </summary>
	public class VariableCollection : IEnumerable<Variable> {

		[SuppressMessage("", "CS8618", Justification = "This field is set by VisualProgram during it's constructor.")]
		internal VisualProgram context;

		private readonly Dictionary<string, Variable> definitions = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// A parameter that will be the first parameter of all compiled functions which provides them access to their instance context (e.g. allows for accessing variable store).
		/// </summary>
		internal readonly ParameterExpression compiledInstanceParameter = Expression.Parameter(typeof(ICompiledInstanceBase), "context");

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
		/// Adds the given variable to this collection.
		/// </summary>
		internal void Add(Variable variable) {
			if (definitions.ContainsKey(variable.Name))
				throw new ArgumentException($"A variable with the name '{variable.Name}' already exists.", nameof(variable));
			definitions.Add(variable.Name, variable);
		}

		/// <summary>
		/// Returns a boolean indicating if a variable by the given name exists in this collection.
		/// </summary>
		public bool Contains(string name) => definitions.ContainsKey(name);

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
		/// <returns><c>True</c> if the variable was deleted, <c>false</c> if there is no variable by this name or the variable is locked.</returns>
		public bool Remove(string name) {
			if (definitions.Remove(name) && !IsVariableLocked(name)) {
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
				return true;
			}
			return false;
		}

		/// <summary>
		/// Merges the locked variables from the environment into this collection.<para/>
		/// Will throw an error if a variable of the same name but different types exist in this collection and the environment.
		/// In the case of mis-matching default values for a variable of the same name and type, the default from the collection will be kept.
		/// </summary>
		/// <exception cref="InvalidOperationException">If there is a variable with the same name but different type in both the environment's locked variables and this collection.</exception>
		internal void MergeWithEnvironment() {
			foreach (var locked in context.Environment.LockedVariables) {
				// If the variable already exists in this collection
				if (TryGetVariable(locked.Name, out var thisVar)) {
					// And if the types are mismatching, throw an error.
					if (thisVar.Type != locked.Type)
						throw new InvalidOperationException($"A variable conflict was found between the environment's locked variables and the variables defined in this collection. Variable '{locked.Name}' is defined on the collection as a '{thisVar.Type.Name}', but the environment defines it as a '{locked.Type.Name}'.");

					// If the types match, we don't need to do anything since the one in this collection is valid 
					// (we will keep the default defined here instead of the default defined on the locked variable)

				} else {
					// Otherwise if the varaible does not exist in our collection, add it
					definitions.Add(locked.Name, locked);
				}
			}
		}

		/// <summary>
		/// Determines if a variable with the given name is locked by the context's environment.
		/// </summary>
		internal bool IsVariableLocked(string varName) => context.Environment.LockedVariables.Any(v => v.Name.Equals(varName, StringComparison.OrdinalIgnoreCase));

		#region IEnumerable
		public IEnumerator<Variable> GetEnumerator() => definitions.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		#endregion
	}
}
