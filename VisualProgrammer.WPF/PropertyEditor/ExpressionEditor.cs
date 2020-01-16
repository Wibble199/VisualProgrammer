using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.Util;

namespace VisualProgrammer.WPF.PropertyEditor {

	/// <summary>
	/// Control that provides an editor for setting a <see cref="Core.VisualNodePropertyType.Expression"/>-type property.
	/// </summary>
	[TemplatePart(Name = PART_ExpressionConnector, Type = typeof(VisualNodeConnector))]
	public class ExpressionEditor : Control {

		private const string PART_ExpressionConnector = "PART_ExpressionConnector";

		static ExpressionEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionEditor), new FrameworkPropertyMetadata(typeof(ExpressionEditor)));
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();
			if (GetTemplateChild(PART_ExpressionConnector) is VisualNodeConnector e) {
				e.MouseDown += StartConnectorDrag;
				e.MouseUp += EndConnectorDrag;
			}
		}

		private void StartConnectorDrag(object sender, MouseButtonEventArgs e) {
			// Check both the canvas and the context exist
			if (DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this) is VisualNodeCanvas canvas)
				// Tell the canvas that the user has started dragging this node
				canvas.StartDrag((VisualNodeConnector)sender);
		}

		private void EndConnectorDrag(object sender, MouseButtonEventArgs e) {
			// Check both the canvas and the context exist
			if (DependencyObjectUtils.AncestorOfType<VisualNodeCanvas>(this) is VisualNodeCanvas canvas)
				// Tell the canvas that the user has started dragging this node
				canvas.EndDrag((VisualNodeConnector)sender);
		}
	}
}
