using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace VisualProgrammer.WPF.Util {

	/// <summary>
	/// Converter that takes a <see cref="IEnumerable"/> and an object value and which returns the index of that item in the collection.
	/// </summary>
	public class EnumerableIndexConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (values.Length != 2 || !(values[0] is IEnumerable enumerable))
				return -1;
			var i = 0;
			foreach (var item in enumerable) {
				if (Equals(item, values[1]))
					return i;
				i++;
			}
			return -1;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}

	/// <summary>
	/// Converter that takes an <see cref="IEnumerable"/> and returns the number of elements it contains.
	/// </summary>
	public class EnumerableCountConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is ICollection c) return c.Count;
			if (value is IEnumerable e) {
				var i = 0;
				foreach (var _ in e)
					i++;
				return i;
			}
			return -1;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
