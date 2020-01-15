using System;
using System.Collections.Generic;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF.PropertyEditor {

	/// <summary>
	/// Class that stores the definition for a visual node's property (bound to that visual node) and exposes it's
	/// <see cref="VisualNode.GetPropertyValue(string)"/> and <see cref="VisualNode.SetPropertyValue(string, object)"/> methods as
	/// a bindable property This type is passed as the DataContext for a single property in the editor.
	/// </summary>
	public class BoundVisualNodePropertyContext {

		public BoundVisualNodePropertyContext(KeyValuePair<Guid, VisualNode> node, VisualNodePropertyDefinition def) {
			NodeId = node.Key;
			Node = node.Value;
			Definition = def;
		}

		/// <summary>
		/// The unique ID of this node in the VisualProgram.
		/// </summary>
		public Guid NodeId { get; }

		/// <summary>
		/// Gets the node instance that this property is bound to/exists on.
		/// </summary>
		public VisualNode Node { get; }

		/// <summary>
		/// Gets the definition of this property, which contains metadata about the property.
		/// </summary>
		public VisualNodePropertyDefinition Definition { get; }

		/// <summary>
		/// Gets or sets the value of this property on the bound VisualNode instance.
		/// </summary>
		public object Value {
			get => Node.GetPropertyValue(Definition.Name);
			set => Node.SetPropertyValue(Definition.Name, value);
		}
	}
}
