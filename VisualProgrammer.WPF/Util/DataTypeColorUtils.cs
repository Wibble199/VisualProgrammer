using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VisualProgrammer.WPF.Util {

	internal static class DataTypeColorUtils {

		/// <summary>
		/// Resolves the line/node color for the given type.
		/// </summary>
		internal static Color ColorForDataType(this FrameworkElement el, Type type) =>
			type != null && el.TryFindResource(new NodeTypeColorKey(type)) is Color color ? color : Colors.White;
	}


	/// <summary>
	/// Specialised key for specifying a color the visual node should appear in for a particular type (when using the default template).
	/// </summary>
	public class NodeTypeColorKey : ComponentResourceKey {

		/// <summary>
		/// Creates a new <see cref="NodeTypeColorKey"/> that marks the color for the nodes of the given type (for the default template).
		/// </summary>
		/// <param name="targetType">The type that this color is to be used for.</param>
		public NodeTypeColorKey(Type targetType)
			: base(typeof(NodeTypeColorKey), targetType) { }
	}


	/// <summary>
	/// MarkupExtension that takes a binding that points to a type and returns a SolidColorBrush of the color of that type as defined in the color dictionary.
	/// </summary>
	public class DataTypeColorBrushBinding : MultiBinding {

		private static readonly Binding selfBinding = new Binding { RelativeSource = new RelativeSource { Mode = RelativeSourceMode.Self } };
		private static readonly IMultiValueConverter conv = new DataTypeColorConverter();

		public DataTypeColorBrushBinding(Binding typeBinding) {
			Bindings.Add(selfBinding);
			Bindings.Add(typeBinding);
			Converter = conv;
			Mode = BindingMode.OneWay;
		}

		/// <summary>
		/// Converter for the DataTypeColorBrushBinding. Takes a FrameworkElement as the first parameter and a Type as the second to return the SolidColorBrush.
		/// </summary>
		class DataTypeColorConverter : IMultiValueConverter {
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => new SolidColorBrush(
				values[0] is FrameworkElement fe && values[1] is Type type ? fe.ColorForDataType(type) : Colors.White
			);

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
		}
	}
}
