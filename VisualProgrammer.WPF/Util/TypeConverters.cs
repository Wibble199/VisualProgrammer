using System;
using System.Globalization;
using System.Windows.Data;

namespace VisualProgrammer.WPF.Util {

	/// <summary>
	/// Converter that calls <see cref="object.GetType"/> on the value and returns that.
	/// </summary>
	public class GetTypeConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value?.GetType();
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}


	/// <summary>
	/// Returns true if the <see cref="object.GetType"/> extends the Type given as a parameter.
	/// </summary>
	public class TypeIsConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => parameter is Type t && t.IsAssignableFrom(value?.GetType());
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
