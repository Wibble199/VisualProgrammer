using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF.PropertyEditor {

	/// <summary>
	/// Control that provides an editor for setting a <see cref="Core.VisualNodePropertyType.Variable"/>-type property.
	/// </summary>
	public class VariableEditor : Control {

		static VariableEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VariableEditor), new FrameworkPropertyMetadata(typeof(VariableEditor)));
		}
	}
}
