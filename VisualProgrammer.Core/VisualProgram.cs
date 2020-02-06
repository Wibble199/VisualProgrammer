using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using VisualProgrammer.Core.Compilation;
using VisualProgrammer.Core.Environment;

namespace VisualProgrammer.Core {

	/// <summary>
	/// The class that is responsible for storing and editing program data which can be compiled.
	/// </summary>
	public sealed class VisualProgram {

		public VisualProgram() : this(_ => { }) { }
		public VisualProgram(Action<VisualProgramEnvironmentBuilder> environmentConfig) {
			var builder = new VisualProgramEnvironmentBuilder();
			environmentConfig(builder);
			Environment = builder.Build();
		}

		public VisualProgramEnvironment Environment { get; }

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
                throw new ArgumentException($"Supplied node type must be a '{nameof(VisualNode)}'.", nameof(nodeType));
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

        #region Variables
		public Dictionary<string, Variable> variableDefinitions = new Dictionary<string, Variable>();
		/// <summary>
		/// The recognised variables for this program. The dictionary key is the name of the variable, and the dictionary value is a tuple containing the variable type and default value.
		/// </summary>
		public ReadOnlyDictionary<string, Variable> VariableDefinitions => new ReadOnlyDictionary<string, Variable>(variableDefinitions);

		/// <summary>
		/// A parameter that will be the first parameter of all compiled functions which provides them access to their instance context (e.g. allows for accessing variable store).
		/// </summary>
		internal readonly ParameterExpression compiledInstanceParameter = Expression.Parameter(typeof(CompiledInstanceBase), "context");

		/// <summary>
		/// Attempts to define a new variable on this program.
		/// </summary>
		/// <exception cref="ArgumentException">If the new variable's name is invalid or if the <paramref name="default"/> value cannot be assigned to type <paramref name="type"/>.</exception>
		/// <exception cref="ArgumentNullException">If the new variable's type is <c>null</c>.</exception>
		public void CreateVariable(string name, Type type, object? @default) {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name must not be null, empty or whitespace.", nameof(name));
			if (variableDefinitions.ContainsKey(name))
				throw new ArgumentException($"A variable with the name '{name}' already exists.", nameof(name));
			if (type == null)
				throw new ArgumentNullException(nameof(type), "Variable type must not be null.");
			variableDefinitions.Add(name, new Variable(type, @default));
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
					prop.Setter(node, VariableReference.Create(prop.PropertyDataType!, "")); // Create a new empty reference of the relevant type
			}
		}
		#endregion

        /// <summary>
        /// Compiles the VisualProgram into a compiled program factory, which can then be used to create independent instances of the program.<para/>
		/// The program instances generated from this factory will attempt to implement the given interface.
        /// </summary>
        public CompiledProgramFactory<TImplements> Compile<TImplements>() where TImplements : class => new CompiledProgramFactory<TImplements>(
			Nodes.Values.OfType<VisualEntry>().ToDictionary(e => e.VisualEntryId, e => e.CreateLambda(this).Compile()),
			variableDefinitions!
		);

		/// <summary>
		/// Compiles the VisualProgram into a compiled program factory, which can then be used to create independent instances of the program.
		/// </summary>
		public CompiledProgramFactory<IAnonymousProgram> Compile() => Compile<IAnonymousProgram>();
    }
}
