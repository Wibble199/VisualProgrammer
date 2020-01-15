using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.Util;

namespace VisualProgrammer.WPF {

	[TemplatePart(Name = PART_ExpressionReturnConnector, Type = typeof(Ellipse))]
    public class VisualNodePresenter : Control {

		private const string PART_ExpressionReturnConnector = "PART_ExpressionReturnConnector";

		static VisualNodePresenter() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodePresenter), new FrameworkPropertyMetadata(typeof(VisualNodePresenter)));
        }

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			if (GetTemplateChild(PART_ExpressionReturnConnector) is Ellipse connector) {
				connector.MouseDown += StartConnectorDrag;
				connector.MouseUp += EndConnectorDrag;
			}
		}

		private void StartConnectorDrag(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			// Check both the canvas and the context exist
			if (DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this) is VisualNodeCanvas canvas && DataContext is KeyValuePair<Guid, VisualNode> context)
				// Tell the canvas that the user has started dragging this node
				canvas.EndDrag(GetConnectorData(context));
		}

		private void EndConnectorDrag(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			// Check both the canvas and the context exist
			if (DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this) is VisualNodeCanvas canvas && DataContext is KeyValuePair<Guid, VisualNode> context)
				// Tell the canvas that the user has started dragging this node
				canvas.EndDrag(GetConnectorData(context));
		}

		private ConnectorData GetConnectorData(KeyValuePair<Guid, VisualNode> context) => new ConnectorData {
			nodeId = context.Key,
			node = context.Value,
			type = context.Value is VisualStatement ? VisualNodePropertyType.Statement : VisualNodePropertyType.Expression,
			name = "",
			isInput = false // Since this connector is on the node presenter, it is always an output
		};
	}
}
