using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF {

	public class VariableListEditor : Control {
		static VariableListEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VariableListEditor), new FrameworkPropertyMetadata(typeof(VariableListEditor)));
		}
	}
}
