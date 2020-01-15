using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF.PropertyEditor {

	/// <summary>
	/// A control that gets the relevant type of editor for the type of property in the DataContext.<para />
	/// Should only be used inside a <see cref="MultiEditor"/>.
	/// </summary>
	public class SingleEditor : ContentControl {
		static SingleEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SingleEditor), new FrameworkPropertyMetadata(typeof(SingleEditor)));
		}
	}
}
