using System.Windows;
using System.Windows.Media;

namespace VisualProgrammer.WPF.Util {

	public static class DependencyObjectUtils {

		/// <summary>
		/// Finds an ancestor (that is of the target type) for this dependency object.
		/// Returns null if no ancestor of this type exists.
		/// </summary>
		/// <typeparam name="T">The type of ancestor to search for.</typeparam>
		public static T AncestorOfType<T>(DependencyObject self) where T : DependencyObject {
			var obj = self;
			do {
				obj = VisualTreeHelper.GetParent(obj);
				if (obj is T tObj)
					return tObj;
			} while (obj != null);
			return null;
		}
	}
}
