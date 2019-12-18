using System;
using System.Collections.Generic;

namespace VisualProgrammer.Blazor {

    public static class Utils {

        /// <summary>
        /// Gets the name of the `this` type without the generic information.
        /// For non-generic types, simply returns their name unchanged.
        /// <code>SomeGeneric&lt;T&gt;.Name => SomeGeneric`1<br/>
        /// SomeGeneric&lt;T&gt;.GetNameWithoutGeneric() => SomeGeneric
        /// </code>
        /// </summary>
        public static string GetNameWithoutGeneric(this Type type) => type.IsGenericType ? type.Name.Substring(0, type.Name.IndexOf('`')) : type.Name;

        /// <summary>
        /// Adds or removes an item from the collection depending on if it already exists in the collection or not.
        /// </summary>
        public static void Toggle<T>(this ICollection<T> collection, T value) {
            if (collection.Contains(value))
                collection.Remove(value);
            else
                collection.Add(value);
        }
    }
}
