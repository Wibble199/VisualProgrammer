using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF.ViewModels {

	/// <summary>
	/// A view-model for a single <see cref="VisualNode"/> in the program.
	/// </summary>
	public sealed class VisualNodeViewModel : ViewModelBase<VisualNode> {

		// Lazily-evaluated property lists. These need to only be evaluated once so that we can ensure we only create one view-model per property.
		private readonly Lazy<IEnumerable<VisualNodePropertyViewModel>> listableProperties;
		private readonly Lazy<IEnumerable<VisualNodePropertyViewModel>> linkableProperties;
		private readonly Lazy<IEnumerable<VisualNodePropertyViewModel>> statementProperties;

		internal VisualNodeViewModel(VisualNode model, Guid id) : base(model) {
			ID = id;

			//// Initialise lazy properties
			// Note that these are being cast 'ToArray'ed so that the IEnumerable does not get re-evaluated every time it is accessed (because otherwise this causes the view
			// models to be different instances EVERY time you would access the property, so listableProperties.Value.First() == listableProperties.Value.First() would be false)
			listableProperties = new Lazy<IEnumerable<VisualNodePropertyViewModel>>(() => PropsOfType(VisualNodePropertyType.Expression, VisualNodePropertyType.Variable, VisualNodePropertyType.Value)
				.OrderBy(p => p.Meta.Order)
				.ThenBy(p => p.DisplayName)
				.ToArray()
			);

			linkableProperties = new Lazy<IEnumerable<VisualNodePropertyViewModel>>(() => PropsOfType(VisualNodePropertyType.Expression, VisualNodePropertyType.Statement).ToArray());
			statementProperties = new Lazy<IEnumerable<VisualNodePropertyViewModel>>(() => PropsOfType(VisualNodePropertyType.Statement).ToArray());
		}

		/// <summary>The read-only ID of this node.</summary>
		public Guid ID { get; }

		/// <summary>Gets the name of the type of this node.</summary>
		public string NodeName => model.GetType().Name;

		/// <summary>Gets or sets the position of this node in the canvas.</summary>
		public Point Position {
			get => model.Position;
			set => SetAndNotify(model => model.Position, value);
		}

		// Booleans for easily checking the type of node this is in WPF.
		public bool IsExpression => model is IVisualExpression;
		public bool IsStatement => model is VisualStatement;
		public bool IsEntry => model is VisualEntry;


		#region Properties Lists
		private IEnumerable<VisualNodePropertyViewModel> PropsOfType(params VisualNodePropertyType[] types) => model.GetPropertiesOfType(types).Select(prop => new VisualNodePropertyViewModel(prop, model));

		// NOTE: These does not need to be observable since these are always the same for a particular Node class.

		/// <summary>Gets a list of all properties that will appear in the list of the node's visual element (Expressions, Variables and Values).</summary>
		public IEnumerable<VisualNodePropertyViewModel> ListableProperties => listableProperties.Value;

		/// <summary>Gets all properties that can be linked to other nodes and may require a line being rendered.</summary>
		public IEnumerable<VisualNodePropertyViewModel> LinkableProperties => linkableProperties.Value;

		/// <summary>Gets all statement type properties that can be linked to other statements.</summary>
		public IEnumerable<VisualNodePropertyViewModel> StatementProperties => statementProperties.Value;
		#endregion
	}
}
