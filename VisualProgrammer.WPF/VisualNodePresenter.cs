using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.Util;

namespace VisualProgrammer.WPF {

	[TemplatePart(Name = PART_ExpressionReturnConnector, Type = typeof(VisualNodeConnector))]
    public class VisualNodePresenter : Control {

		private const string PART_ExpressionReturnConnector = "PART_ExpressionReturnConnector";

		static VisualNodePresenter() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodePresenter), new FrameworkPropertyMetadata(typeof(VisualNodePresenter)));
        }

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			if (GetTemplateChild(PART_ExpressionReturnConnector) is VisualNodeConnector connector) {
				connector.MouseDown += StartConnectorDrag;
				connector.MouseUp += EndConnectorDrag;
			}
		}

		private void StartConnectorDrag(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			// Check both the canvas and the context exist
			if (DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this) is VisualNodeCanvas canvas)
				// Tell the canvas that the user has started dragging this node
				canvas.StartDrag((VisualNodeConnector)sender);
		}

		private void EndConnectorDrag(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			// Check both the canvas and the context exist
			if (DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this) is VisualNodeCanvas canvas)
				// Tell the canvas that the user has started dragging this node
				canvas.EndDrag((VisualNodeConnector)sender);
		}
	}
}
