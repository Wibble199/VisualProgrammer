using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF {

	public class VisualNodePresenter : Control {

		static VisualNodePresenter() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodePresenter), new FrameworkPropertyMetadata(typeof(VisualNodePresenter)));
        }
	}
}
