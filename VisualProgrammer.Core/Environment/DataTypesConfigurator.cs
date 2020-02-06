using System;
using System.Collections.Generic;

namespace VisualProgrammer.Core.Environment {

	/// <summary>
	/// Fluent API for defining which data types are available.<para/>
	/// This will determine the types of variables that can be created and what generic arguments can be used.
	/// </summary>
	public sealed class DataTypesConfigurator {

		internal readonly HashSet<Type> types = new HashSet<Type>();

		internal DataTypesConfigurator() { }

		/// <summary>Adds the given type to the available data types set.</summary>
		public DataTypesConfigurator Add<T>() => Add(typeof(T));

		/// <summary>Adds the given types to the available data types set.</summary>
		public DataTypesConfigurator Add(params Type[] types) {
			foreach (var type in types)
				this.types.Add(type);
			return this;
		}

		/// <summary>Adds the basic default types. This is currently <see cref="int"/>, <see cref="double"/>, <see cref="string"/> and <see cref="bool"/>.</summary>
		public DataTypesConfigurator AddDefault() => Add(typeof(int), typeof(double), typeof(string), typeof(bool));

		/// <summary>Removes the given type from the available data types set.</summary>
		public DataTypesConfigurator Remove<T>() => Remove(typeof(T));

		/// <summary>Removes the given types from the available data types set.</summary>
		public DataTypesConfigurator Remove(params Type[] types) {
			foreach (var type in types)
				this.types.Add(type);
			return this;
		}
	}
}
