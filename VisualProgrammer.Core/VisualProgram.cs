using System;
using System.Collections.Generic;
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
        public VisualNode? ResolveReference(NodeReference reference) => Nodes.TryGetValue(reference.nodeId, out var node) ? node : null;

        /// <summary>
        /// Attempts to resolve the reference. Returns whether the node was able to be resolved, and the node's value in the <paramref name="visualNode"/> out parameter.
        /// </summary>
        public bool TryResolveReference(NodeReference reference, [MaybeNullWhen(false), NotNullWhen(true)] out VisualNode? visualNode) {
            visualNode = null;
            return reference != null && Nodes.TryGetValue(reference.nodeId, out visualNode);
        }

        public Guid CreateNode(Type nodeType, params Type[] genericTypes) {
            if (!typeof(VisualNode).IsAssignableFrom(nodeType))
                throw new ArgumentException($"Supplied node type must be a '${nameof(VisualNode)}'.", nameof(nodeType));
            if (nodeType.GetGenericArguments().Length != genericTypes.Length)
                throw new ArgumentException($"Node type generic argument count must match given generic type parameter count. Expected {nodeType.GetGenericArguments().Length}, got {genericTypes.Length}.", nameof(genericTypes));
            
            var id = Guid.NewGuid();
            Nodes.Add(id, (VisualNode)Activator.CreateInstance(nodeType.IsGenericType ? nodeType.MakeGenericType(genericTypes) : nodeType)!);
            return id;
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
        /// <summary>
        /// The recognised variables for this program. The dictionary key is the name of the variable, and the dictionary value is a tuple containing the variable type and default value.
        /// </summary>
        public Dictionary<string, (Type type, object @default)> VariableDefinitions { get; set; } = new Dictionary<string, (Type, object)>();

        /// <summary>
        /// The current variable storage. The dictionary's key is the name of the variable and the dictionary value is the current value of that variable.
        /// </summary>
        internal Dictionary<string, object> VariableValues { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Resets all the variables currently stored to their default values.
        /// </summary>
        public void ResetVariables() => VariableValues = VariableDefinitions.ToDictionary(v => v.Key, v => v.Value.@default);
        #endregion

        #region NodeTypes
        /// <summary>
        /// Gets a list of all node types that can be added to this program.
        /// </summary>
        public IEnumerable<Type> AvailableNodes { get; } = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => (typeof(IVisualExpression).IsAssignableFrom(t) || typeof(VisualStatement).IsAssignableFrom(t)) && !t.IsAbstract); // Get anything that extends IExpression or VisualStatement but is not abstract

        // TODO: In future add the possibility of adding per-program nodes and removing default blocks
        #endregion

        /// <summary>
        /// Creates a dictionary containing the KEY of the visual entries (not their names) and a compiled delegate that performs the actions specified by the user.
        /// </summary>
        /// <param name="includeUnset">Whether to generated delegates for the entries that have not been added by the program by the user. In this case, a delegate
        /// that performs no action is generated. If this is false, unset entries will not be present in the returned dictionary.</param>
        public Dictionary<string, Delegate> Compile(bool includeUnset = false) {
            var presentEntries = Nodes.OfType<VisualEntry>().ToDictionary(e => e.VisualEntryId, e => e);
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
