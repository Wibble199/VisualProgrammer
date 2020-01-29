using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF {

	public class VisualNodeToolbox : Control {
		static VisualNodeToolbox() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodeToolbox), new FrameworkPropertyMetadata(typeof(VisualNodeToolbox)));
		}
	}
}
