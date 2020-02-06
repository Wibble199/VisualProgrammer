using System;
using VisualProgrammer.Core.Utils;

namespace VisualProgrammer.Core {

	public sealed class Variable {

		private object? value;

		public Variable(Type type) : this(type, null) { }
		public Variable(Type type, object? defaultValue) {
			Type = type ?? throw new ArgumentNullException("The variable definition must have a type.", nameof(type));

			if (!Type.CanBeSetTo(defaultValue))
				throw new ArgumentException($"Value '{defaultValue?.ToString() ?? "null"}' cannot be set as the default value for this variable because it cannot be assigned to type '{Type.Name}'.", nameof(defaultValue));
			value = DefaultValue = defaultValue;
		}

		public Type Type { get; }
		public object? DefaultValue { get; }

		public object? Value {
			get => value;
			set {
				if (!Type.CanBeSetTo(value))
					throw new ArgumentException($"Value of '{value?.ToString() ?? "null"}' cannot be assigned to variable of type '{Type.Name}'.", nameof(value));
				this.value = value;
			}
		}

		public void Reset() => value = DefaultValue;
	}
}
