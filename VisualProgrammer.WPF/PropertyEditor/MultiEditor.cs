using System.Windows;
using System.Windows.Controls;
using VisualProgrammer.Core;
using VisualProgrammer.WPF.ViewModels;

namespace VisualProgrammer.WPF.PropertyEditor {

	/// <summary>
	/// A control that generates a list of properties for the given <see cref="VisualNode"/>.
	/// </summary>
	public class MultiEditor : ItemsControl {
		static MultiEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiEditor), new FrameworkPropertyMetadata(typeof(MultiEditor)));
		}

		#region Node DependencyProperty
		/// <summary>
		/// The Visual Node whose properties an editor will be generated for.
		/// </summary>
		public VisualNodeViewModel Node {
			get => (VisualNodeViewModel)GetValue(NodeProperty);
			set => SetValue(NodeProperty, value);
		}

		public static readonly DependencyProperty NodeProperty =
			DependencyProperty.Register("Node", typeof(VisualNodeViewModel), typeof(MultiEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion
	}
}
