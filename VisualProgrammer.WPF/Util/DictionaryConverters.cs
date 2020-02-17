using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace VisualProgrammer.WPF.Util {

	/// <summary>
	/// Converter that takes a dictionary and a key and returns the value in the dictionary of that key type.
	/// </summary>
	public class DictionaryValueConverter<TKey, TValue> : IMultiValueConverter {
		
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
			// Check there are two values of the value type ([0] = dictionary, [1] = key). If so, try find the value in the dictionary and return it
			values.Length == 2 && values[0] is IReadOnlyDictionary<TKey, TValue> dict && values[1] is TKey key && dict.TryGetValue(key, out var value)
				? value : default(TValue);

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}
