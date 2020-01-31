using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace VisualProgrammer.WPF.InputField {

	/// <summary>
	/// A specialised combobox for use with the <see cref="InputFieldDynamic"/> that displays a list of values for the enum
	/// specified in the data context.
	/// </summary>
	public class EnumPicker : ComboBox {

		private static readonly EnumListConverter enumListConverter = new EnumListConverter();

		public EnumPicker() {
			// Sets the paths based on the property names in the list returned from the converter
			SelectedValuePath = "Value";
			DisplayMemberPath = "Label";

			// Setup binding for the DataContext.InputType <-> ItemsSource
			SetBinding(
				ItemsSourceProperty,
				new Binding(nameof(InputFieldViewModel.InputType)) { Mode = BindingMode.OneWay, Converter = enumListConverter }
			);

			// Setup binding for the DataContext.Value <-> SelectedValue
			SetBinding(
				SelectedValueProperty,
				new Binding(nameof(InputFieldViewModel.Value)) { Mode = BindingMode.TwoWay }
			);
		}
	}

	/// <summary>
	/// Takes an Enum type and returns a list of values in that type.
	/// </summary>
	public class EnumListConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is Type enumType && enumType.IsEnum
			? Enum.GetValues(enumType)
				.Cast<Enum>()
				.Select(e => new {
					Label = (enumType.GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute)?.Description ?? e.ToString(),
					Value = e
				})
			: null;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
