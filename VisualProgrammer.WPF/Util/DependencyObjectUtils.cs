using System;
using System.Collections.Generic;
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

		/// <summary>
		/// Performs a breadth-first search through all children of the start element to find a child of a specific type matching a predicate.
		/// Returns null if no child matching the type parameter and predicate are found.
		/// </summary>
		/// <typeparam name="T">The type of element to search for.</typeparam>
		/// <param name="start">The element to start the search at.</param>
		/// <param name="predicate">A function that will be invoked on any found <typeparamref name="T"/> children.
		/// If the invocation returns true, this child will be returned otherwise the search will continue.</param>
		public static T ChildOfType<T>(DependencyObject start, Func<T, bool> predicate) where T : DependencyObject {
			var itemsToSearch = new Queue<DependencyObject>();
			itemsToSearch.Enqueue(start);

			while (itemsToSearch.Count > 0) {
				var item = itemsToSearch.Dequeue();
				var childCount = VisualTreeHelper.GetChildrenCount(item);
				for (var i = 0; i < childCount; i++) {
					var child = VisualTreeHelper.GetChild(item, i);
					if (child is T tChild && predicate(tChild))
						return tChild;
					itemsToSearch.Enqueue(child);
				}
			}

			return null;
		}
	}
}
