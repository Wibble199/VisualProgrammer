using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VisualProgrammer.Core {

	/// <summary>
	/// Represents and manages a collection of <see cref="VisualNode" />s.
	/// </summary>
	public class VisualNodeCollection : IEnumerable<VisualNode> {

		private readonly Dictionary<Guid, VisualNode> nodes = new Dictionary<Guid, VisualNode>();

		/// <summary>
		/// Adds the given node to the collection with the given ID (will also set the Id property on the node).<para/>
		/// Will disallow duplicate nodes to be added (i.e. no two references to the same node). If the given ID is an empty guid, a new one is generated for it.
		/// </summary>
		public void Add(Guid id, VisualNode node) {
			// Check node and ID are valid
			if (nodes.ContainsValue(node))
				throw new ArgumentException("This node already exists in the collection.", nameof(node));
			if (nodes.ContainsKey(id))
				throw new ArgumentException("Another node with this ID already exists in the collection.", nameof(id));

			// If node is entry, check that there isn't an entry bound to the same entry definition
			if (node is VisualEntry ve && nodes.Values.OfType<VisualEntry>().Any(e => e.VisualEntryId.Equals(ve.VisualEntryId, StringComparison.OrdinalIgnoreCase)))
				throw new ArgumentException($"A VisualEntry bound to the same entry ('{ve.VisualEntryId}') as the given node.", nameof(node));

			// Create ID if not given one and add node to collection
			if (id == default)
				id = Guid.NewGuid();
			node.Id = id;
			nodes[id] = node;
		}

		/// <summary>
		/// Adds the given node to the collection using it's pre-defined key. If the ID is an empty guid, will create and set a new one.
		/// </summary>
		public void Add(VisualNode node) => Add(node.Id, node);

		/// <summary>
		/// Creates and adds a new visual node of the given type to the collection.
		/// </summary>
		/// <param name="type">The type of node to create. Must be an <see cref="IVisualNode"/>.</param>
		/// <param name="genericTypes">If the given node type is generic, these are the types </param>
		public VisualNode Create(Type type, params Type[] genericTypes) {
			var vn = VisualNode.Construct(type, genericTypes);
			Add(vn);
			return vn;
		}

		/// <summary>
		/// Creates and adds a new visual entry node that is bound to the given entry definition.
		/// </summary>
		public VisualEntry Create(EntryDefinition entryDefinition) {
			var node = new VisualEntry(entryDefinition.Id);
			Add(node);
			return node;
		}

		/// <summary>
		/// Attempts to get the node with the given ID. If the ID is null or a node with that ID doesn't exist, returns null.
		/// </summary>
		public VisualNode? this[Guid? id] => id.HasValue && nodes.TryGetValue(id.Value, out var node) ? node : null;

		/// <summary>
		/// Attempts to get the node with the given ID. Returns true if a node was found, false otherwise.
		/// </summary>
		public bool TryGetNode(Guid id, [MaybeNullWhen(false), NotNullWhen(true)] out VisualNode? visualNode) => nodes.TryGetValue(id, out visualNode);

		/// <summary>
		/// Removes the given node from the progam, unlinking any connected nodes.
		/// </summary>
		public void Remove(VisualNode node) => Remove(node.Id);

		/// <summary>
		/// Removes the node with the given ID from the progam, unlinking any connected nodes.
		/// </summary>
		public void Remove(Guid nodeId) {
			if (!nodes.ContainsKey(nodeId)) return;
			foreach (var node in nodes.Values)
				node.ClearAllLinks(nodeId);
			nodes.Remove(nodeId);
		}

		#region IEnumerable
		public IEnumerator<VisualNode> GetEnumerator() => nodes.Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		#endregion
	}
}
