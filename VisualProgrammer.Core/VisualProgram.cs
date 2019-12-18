using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core {

    public sealed class VisualProgram {

		#region Nodes
		/// <summary>
		/// A list of all nodes in the program, stored by their unique identifiers.
		/// </summary>
		public Dictionary<Guid, VisualNode> Nodes { get; set; } = new Dictionary<Guid, VisualNode>();

        /// <summary>
        /// Attempts to resolve the link and return the linked node. Will return null if no node could be found.
        /// </summary>
        public VisualNode? ResolveReference(Guid? id) => id.HasValue && Nodes.TryGetValue(id.Value, out var node) ? node : null;

        /// <summary>
        /// Attempts to resolve the reference. Returns whether the node was able to be resolved, and the node's value in the <paramref name="visualNode"/> out parameter.
        /// </summary>
        public bool TryResolveReference(Guid id, [MaybeNullWhen(false), NotNullWhen(true)] out VisualNode? visualNode) => Nodes.TryGetValue(id, out visualNode);

		/// <summary>
		/// Attempt to create a new node of the given type. Returns the ID of the newly created node.
		/// </summary>
		/// <param name="nodeType">The type of node to create. This must extend from <see cref="VisualNode"/>.</param>
		/// <param name="genericTypes">If the node type is generic, pass the desired generic types here.</param>
        public Guid CreateNode(Type nodeType, params Type[] genericTypes) {
			// Check the given type is actually a VisualNode
            if (!typeof(VisualNode).IsAssignableFrom(nodeType))
                throw new ArgumentException($"Supplied node type must be a '${nameof(VisualNode)}'.", nameof(nodeType));
			// Check the given type's generic parameter count matches the given genericTypes param array count
            if (nodeType.GetGenericArguments().Length != genericTypes.Length)
                throw new ArgumentException($"Node type generic argument count must match given generic type parameter count. Expected {nodeType.GetGenericArguments().Length}, got {genericTypes.Length}.", nameof(genericTypes));
            
            var id = Guid.NewGuid();
            Nodes.Add(id, (VisualNode)Activator.CreateInstance(nodeType.IsGenericType ? nodeType.MakeGenericType(genericTypes) : nodeType)!);
            return id;
        }

		/// <summary>
		/// Attempts to delete the node with the given ID. Will also remove the node from any links it is associated with.
		/// </summary>
		public void RemoveNode(Guid nodeId) {
			if (!Nodes.ContainsKey(nodeId)) return;
			foreach (var node in Nodes.Values)
				node.ClearAllLinks(nodeId);
			Nodes.Remove(nodeId);
		}
        #endregion

        #region Entries
        /// <summary>
        /// Contains a list of available entry definitions in this program. Any entry defined in here will be able to be added to the program
        /// and will be available for the user to use.<para/>
        /// Note that entries are serialized by their IDs, so the keys of the dictionary should generally be kept the constant between updates.</summary>
        /// <remarks>This should not be serialized, but set directly after/during serialization or the constructor.</remarks>
        public Dictionary<string, EntryDefinition> EntryDefinitions { get; set; } = new Dictionary<string, EntryDefinition>();
        #endregion

        #region Variables
		public Dictionary<string, (Type type, object? @default)> variableDefinitions = new Dictionary<string, (Type, object?)>();
		/// <summary>
		/// The recognised variables for this program. The dictionary key is the name of the variable, and the dictionary value is a tuple containing the variable type and default value.
		/// </summary>
		public ReadOnlyDictionary<string, (Type type, object? @default)> VariableDefinitions => new ReadOnlyDictionary<string, (Type type, object? @default)>(variableDefinitions);

        /// <summary>
        /// The current variable storage. The dictionary's key is the name of the variable and the dictionary value is the current value of that variable.
        /// </summary>
        internal Dictionary<string, object?> VariableValues { get; private set; } = new Dictionary<string, object?>();

        /// <summary>
        /// Resets all the variables currently stored to their default values.
        /// </summary>
        public void ResetVariables() => VariableValues = VariableDefinitions.ToDictionary(v => v.Key, v => v.Value.@default);

		public void CreateVariable(string name, Type type, object? @default) {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name must not be null, empty or whitespace.", nameof(name));
			if (type == null)
				throw new ArgumentNullException(nameof(type), "Variable type must not be null.");
			if (!type.CanBeSetTo(@default))
				throw new ArgumentException($"Value '{@default?.ToString()}' cannot be set as the default value for this variable because it cannot be assigned to type '{type.Name}'.", nameof(@default));
			if (variableDefinitions.ContainsKey(name))
				throw new Exception(); // TODO: Throw error if variable with name already exists
			variableDefinitions.Add(name, (type, @default));
			VariableValues.Add(name, @default);
		}

		/// <summary>
		/// Removes the variable with the given name from the <see cref="VariableDefinitions"/> dictionary.<para/>
		/// Will remove the references from any VisualEntry parameter mappings and any VisualNode's VariableReference properties
		/// </summary>
		public void RemoveVariable(string name) {
			if (variableDefinitions.Remove(name)) {
				// References to reset (cannot remove in the first loop as doing so modifies the collection and invalidates the enumerator)
				var paramMapsToReset = new List<(VisualEntry, string)>();
				var varRefsToReset = new List<(VisualNode, VisualNodePropertyDefinition)>();

				// If removal was successful, clean up some references to the variable
				foreach (var (_, node) in Nodes) {
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
					prop.Setter(node, Activator.CreateInstance(typeof(VariableReference<>).MakeGenericType(prop.PropertyDataType), "")); // Create a new empty reference of the relevant type

				// Remove the variable from the runtime store
				VariableValues.Remove(name);
			}
		}
		#endregion

		#region NodeTypes
		/// <summary>
		/// Gets a list of all node types that can be added to this program.
		/// </summary>
		public IEnumerable<Type> AvailableNodes { get; } = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => (typeof(IVisualExpression).IsAssignableFrom(t) || typeof(VisualStatement).IsAssignableFrom(t)) && !t.IsAbstract); // Get anything that extends IExpression or VisualStatement but is not abstract

		// TODO: In future add the possibility of adding per-program nodes and removing default blocks
		
		/// <summary>
		/// Gets a list of all types that are supported for use as variables or as generic arguments for nodes.
		/// </summary>
		// TODO: Add some way of adding additional types?
		public IEnumerable<Type> AvailableDataTypes { get; } = new[] { typeof(string), typeof(double), typeof(int), typeof(bool) };
        #endregion

        /// <summary>
        /// Creates a dictionary containing the KEY of the visual entries (not their names) and a compiled delegate that performs the actions specified by the user.
        /// </summary>
        /// <param name="includeUnset">Whether to generated delegates for the entries that have not been added by the program by the user. In this case, a delegate
        /// that performs no action is generated. If this is false, unset entries will not be present in the returned dictionary.</param>
        public Dictionary<string, Delegate> Compile(bool includeUnset = false) {
            var presentEntries = Nodes.Values.OfType<VisualEntry>().ToDictionary(e => e.VisualEntryId, e => e);
            return includeUnset
                ? EntryDefinitions.ToDictionary(
                    e => e.Key,
                    e => (presentEntries.TryGetValue(e.Key, out var ve) ? ve : new VisualEntry(e.Key)).CreateLambda(this).Compile()
                )
                : presentEntries.ToDictionary(
                    e => e.Key,
                    e => e.Value.CreateLambda(this).Compile()
                );
        }
    }


    /// <summary>
    /// Class that defines a single entry point for a VisualProgram.
    /// </summary>
    public sealed class EntryDefinition {
		public string Name { get; set; } = "";

        /// <summary>
        /// Specifies the parameters that will be passed to this entry. This will define the signature of the compiled delegate generated by this VisualEntry.<para/>
        /// Note that it IS safe to re-order parameters between versions without breaking existing programs (since the lambda is recompiled at runtime), however it IS
        /// NOT safe to rename or change the type of existing parameters (because this may break existing parameter maps in the VisualEntry instances). It is also safe
        /// to add new parameters without affecting existing programs.
        /// </summary>
        public IndexedDictionary<string, Type> Parameters { get; set; } = new IndexedDictionary<string, Type>();
    }
}
