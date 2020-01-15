using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VisualProgrammer.Core;

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
		public KeyValuePair<Guid, VisualNode>? Node {
			get => (KeyValuePair<Guid, VisualNode>)GetValue(NodeProperty);
			set => SetValue(NodeProperty, value);
		}

		public static readonly DependencyProperty NodeProperty =
			DependencyProperty.Register("Node", typeof(KeyValuePair<Guid, VisualNode>?), typeof(MultiEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		#endregion
	}


	/// <summary>
	/// Converter that takes a <see cref="VisualNode"/> and returns a collection of sorted <see cref="BoundVisualNodePropertyContext"/> objects, one for
	/// each editable property of the node.
	/// </summary>
	public class NodeToPropertyListConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is KeyValuePair<Guid, VisualNode> v
				? ((IVisualNode)v.Value).GetPropertiesOfTypes(VisualNodePropertyType.Expression, VisualNodePropertyType.Value, VisualNodePropertyType.Variable)
					.OrderBy(p => p.Meta.Order)
					.ThenBy(p => p.DisplayName)
					.Select(p => new BoundVisualNodePropertyContext(v, p))
				: null;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
