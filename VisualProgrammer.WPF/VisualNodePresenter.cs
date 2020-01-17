using System;
using System.Collections.Generic;
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


	/// <summary>
	/// Adds an attached property to grids that sets the columns based on a the given value count.
	/// </summary>
	public static class GridColumnCountHelper {

		public static int GetColumnCount(DependencyObject obj) => (int)obj.GetValue(ColumnCountProperty);
		public static void SetColumnCount(DependencyObject obj, int value) => obj.SetValue(ColumnCountProperty, value);

		public static readonly DependencyProperty ColumnCountProperty =
			DependencyProperty.RegisterAttached("ColumnCount", typeof(int), typeof(GridColumnCountHelper), new PropertyMetadata(0, ColumnCountChanged));

		public static void ColumnCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if (!(obj is Grid grid)) return;
			grid.ColumnDefinitions.Clear();
			for (var i = 0; i < (int)e.NewValue; i++)
				grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		}
	}
}
