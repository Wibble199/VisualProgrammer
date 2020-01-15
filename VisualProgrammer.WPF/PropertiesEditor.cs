using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VisualProgrammer.Core;

namespace VisualProgrammer.WPF {

	/// <summary>
	/// A control that generates a list of properties for the given <see cref="IVisualNode"/>.
	/// </summary>
	public class PropertiesEditor : ItemsControl {
		static PropertiesEditor() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertiesEditor), new FrameworkPropertyMetadata(typeof(PropertiesEditor)));
		}

		#region Node DependencyProperty
		/// <summary>
		/// The Visual Node whose properties an editor will be generated for.
		/// </summary>
		public IVisualNode Node {
			get => (IVisualNode)GetValue(NodeProperty);
			set => SetValue(NodeProperty, value);
		}

		public static readonly DependencyProperty NodeProperty =
			DependencyProperty.Register("Node", typeof(IVisualNode), typeof(PropertiesEditor), new PropertyMetadata(null));
		#endregion
	}


	/// <summary>
	/// Converter that takes a <see cref="IVisualNode"/> and returns a collection of sorted anonymous objects with a <see cref="VisualNodePropertyDefinition"/>
	/// "Def" and a <see cref="VisualNodePropertyProxy"/> "Proxy" that are part of the node.
	/// </summary>
	public class NodeToPropertyListConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is IVisualNode v
				? v.GetPropertiesOfTypes(VisualNodePropertyType.Expression, VisualNodePropertyType.Value, VisualNodePropertyType.Variable)
					.OrderBy(p => p.Meta.Order)
					.ThenBy(p => p.DisplayName)
					.Select(p => new { Def = p, Proxy = new VisualNodePropertyProxy(v, p.Name) })
				: null;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}


	/// <summary>
	/// Class that exposes <see cref="IVisualNode.GetPropertyValue(string)"/> and <see cref="IVisualNode.SetPropertyValue(string, object)"/> for a
	/// particular node and property of that node as a property.
	/// </summary>
	public class VisualNodePropertyProxy {

		private readonly IVisualNode node;
		private readonly string name;

		public VisualNodePropertyProxy(IVisualNode node, string name) {
			this.node = node;
			this.name = name;
		}

		public object Value {
			get => node.GetPropertyValue(name);
			set => node.SetPropertyValue(name, value);
		}
	}
}
