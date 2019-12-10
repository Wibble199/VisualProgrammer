using System;

namespace VisualProgrammer.Core.Utils {

	public static class TypeUtils {

		/// <summary>Determines whether the given value is acceptable by the this type.</summary>
		public static bool CanBeSetTo(this Type type, object? value)
			=> (value == null && (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)) // If the value is null, check the type is not a value type (e.g. it is a class) OR it is a Nullable<T>.
			|| (value != null && type.IsAssignableFrom(value.GetType())); // If the value is not null, check that it's type can be assigned to 'this' type
	}
}
