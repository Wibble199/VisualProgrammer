using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF {

	public class PropertyEditor : ContentControl {
		static PropertyEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditor), new FrameworkPropertyMetadata(typeof(PropertyEditor)));
		}
	}
}
