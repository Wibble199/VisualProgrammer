﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.Util;

namespace VisualProgrammer.WPF.PropertyEditor {

	/// <summary>
	/// Control that provides an editor for setting a <see cref="Core.VisualNodePropertyType.Expression"/>-type property.
	/// </summary>
	[TemplatePart(Name = PART_ExpressionConnector, Type = typeof(Ellipse))]
	public class ExpressionEditor : Control {

		private const string PART_ExpressionConnector = "PART_ExpressionConnector";

		static ExpressionEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionEditor), new FrameworkPropertyMetadata(typeof(ExpressionEditor)));
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			if (GetTemplateChild(PART_ExpressionConnector) is Ellipse e) {
				e.MouseDown += StartConnectorDrag;
				e.MouseUp += EndConnectorDrag;
			}
		}

		private void StartConnectorDrag(object sender, MouseButtonEventArgs e) {
			// Check both the canvas and the context exist
			if (DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this) is VisualNodeCanvas canvas && DataContext is BoundVisualNodePropertyContext context)
				// Tell the canvas that the user has started dragging this node
				canvas.StartDrag(GetConnectorData(context));
		}

		private void EndConnectorDrag(object sender, MouseButtonEventArgs e) {
			// Check both the canvas and the context exist
			if (DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this) is VisualNodeCanvas canvas && DataContext is BoundVisualNodePropertyContext context)
				// Tell the canvas that the user has started dragging this node
				canvas.EndDrag(GetConnectorData(context));
		}

		private ConnectorData GetConnectorData(BoundVisualNodePropertyContext context) => new ConnectorData {
			nodeId = context.NodeId,
			node = context.Node,
			type = VisualNodePropertyType.Expression,
			name = context.Definition.Name,
			isInput = true // Since this connector is on the Expression property editor, it is always an input
		};
	}
}
