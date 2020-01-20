using System.Windows;
using VisualProgrammer.WPF.ViewModels;

namespace VisualProgrammer.WPF.AttachedProperties {

	/// <summary>
	/// Defines a <c>VisualProgramModel</c> attached property for framework elements. This property is inherited, so only needs to be set at the highest level.
	/// </summary>
	public class VisualProgramViewModelAttachedProperty {

		public static VisualProgramViewModel GetVisualProgramModel(DependencyObject obj) => (VisualProgramViewModel)obj.GetValue(VisualProgramModelProperty);
		public static void SetVisualProgramModel(DependencyObject obj, VisualProgramViewModel value) => obj.SetValue(VisualProgramModelProperty, value);

		public static readonly DependencyProperty VisualProgramModelProperty =
			DependencyProperty.RegisterAttached("VisualProgramModel", typeof(VisualProgramViewModel), typeof(VisualProgramViewModelAttachedProperty), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
	}
}
