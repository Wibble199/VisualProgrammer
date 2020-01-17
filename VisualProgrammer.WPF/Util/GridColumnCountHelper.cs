using System.Windows;
using System.Windows.Controls;

namespace VisualProgrammer.WPF.Util {

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
