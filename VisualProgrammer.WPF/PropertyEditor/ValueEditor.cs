using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF.PropertyEditor {

	/// <summary>
	/// Control that provides an editor for setting a <see cref="Core.VisualNodePropertyType.Value"/>-type property.
	/// </summary>
	public class ValueEditor : Control {
		static ValueEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ValueEditor), new FrameworkPropertyMetadata(typeof(ValueEditor)));
		}
	}
}
