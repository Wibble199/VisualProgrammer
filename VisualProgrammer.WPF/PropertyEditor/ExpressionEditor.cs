using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF.PropertyEditor {

	/// <summary>
	/// Control that provides an editor for setting a <see cref="Core.VisualNodePropertyType.Expression"/>-type property.
	/// </summary>
	public class ExpressionEditor : Control {
		static ExpressionEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpressionEditor), new FrameworkPropertyMetadata(typeof(ExpressionEditor)));
		}
	}
}
