using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF {

	public class VisualNodePresenter : Control {

		static VisualNodePresenter() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualNodePresenter), new FrameworkPropertyMetadata(typeof(VisualNodePresenter)));
		}
	}


	/// <summary>
	/// Value converter that takes a VisualNode and returns a collection of <see cref="VisualNodePropertyDefinition" />s of the <see cref="VisualNodePropertyType"/> specified in the parameter.
	/// </summary>
	public class VisualNodePropertyListConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value as VisualNode)?.GetPropertiesOfType((VisualNodePropertyType)parameter);
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
