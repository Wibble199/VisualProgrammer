using System;
using System.Globalization;
using System.Windows.Data;

namespace VisualProgrammer.WPF.Util {

	/// <summary>
	/// Converter that returns one of two values based on the input boolean.
	/// </summary>
	public class BooleanValueConverter : IValueConverter {

		public object TrueValue { get; set; }
		public object FalseValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && b ? TrueValue : FalseValue;
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Equals(value, TrueValue);
	}
}
